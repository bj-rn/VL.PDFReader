using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading;
using VL.PDFReader.Internals;
using Stride.Core.Mathematics;
using Stride.Graphics;
using VL.Skia;
using Path = VL.Lib.IO.Path;


namespace VL.PDFReader
{

#pragma warning disable CA1510 // Use ArgumentNullException throw helper
#pragma warning disable CA1513 // Use ObjectDisposedException throw helper

    /// <summary>
    /// Provides functionality to render a PDF document.
    /// </summary>
    public class PdfDocument : IDisposable
    {
        private bool _disposed;
        private PdfFile _file;

        private Int2 _maxTileSize = new Int2(4000, 4000);


        /// <summary>
        /// Size of each page in the PDF document.
        /// </summary>
        public IReadOnlyList<Vector2> PageSizes { get; private set; }


        /// <summary>
        /// Number of pages in the PDF document.
        /// </summary>
        public int PageCount
        {
            get { return PageSizes.Count; }
        }


        /// <summary>
        /// Initializes a new instance of the PdfDocument class with the provided path.
        /// </summary>
        /// <param name="path">path the PDF document.</param>
        /// <param name="password">Password for the PDF document.</param>
        /// <param name="maxTileSize"></param>
        /// <param name="disposeStream">Decides if the stream  <paramref name="path"/> will closed on dispose as well.</param>
        public static PdfDocument Load(Path path, string? password, Int2? maxTileSize, bool disposeStream = true)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));


            return Load(File.OpenRead(path), password, maxTileSize, disposeStream);
        }


        /// <summary>
        /// Initializes a new instance of the PdfDocument class with the provided stream.
        /// </summary>
        /// <param name="stream">Stream for the PDF document.</param>
        /// <param name="password">Password for the PDF document.</param>
        /// <param name="maxTileSize"></param>
        /// <param name="disposeStream">Decides if <paramref name="stream"/> will closed on dispose as well.</param>
        public static PdfDocument Load(Stream stream, string? password, Int2? maxTileSize, bool disposeStream = true)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            return new PdfDocument(stream, password, maxTileSize, disposeStream);
        }

        private PdfDocument(Stream stream, string? password, Int2? maxTileSize, bool disposeStream)
        {
            _file = new PdfFile(stream, password, disposeStream);

            if (maxTileSize.HasValue)
                _maxTileSize = maxTileSize.Value;

            PageSizes = new ReadOnlyCollection<Vector2>(_file.GetPDFDocInfo() ?? throw new Win32Exception());
        }


        /// <summary>
        /// Get metadata information from the PDF document.
        /// </summary>
        /// <returns>The PDF metadata.</returns>
        public PdfMetaData GetMetaData()
        {
            return _file.GetMetaData();
        }


        /// <summary>
        /// Finds all occurences of text.
        /// </summary>
        /// <param name="text">The text to search for.</param>
        /// <param name="matchCase">Whether to match case.</param>
        /// <param name="wholeWord">Whether to match whole words only.</param>
        /// <returns>All matches.</returns>
        public PdfMatches Search(string text, bool matchCase, bool wholeWord)
        {
            return Search(text, matchCase, wholeWord, 0, PageCount - 1);
        }


        /// <summary>
        /// Finds all occurences of text.
        /// </summary>
        /// <param name="text">The text to search for.</param>
        /// <param name="matchCase">Whether to match case.</param>
        /// <param name="wholeWord">Whether to match whole words only.</param>
        /// <param name="page">The page to search on.</param>
        /// <returns>All matches.</returns>
        public PdfMatches Search(string text, bool matchCase, bool wholeWord, int page)
        {
            return Search(text, matchCase, wholeWord, page, page);
        }


        /// <summary>
        /// Finds all occurences of text.
        /// </summary>
        /// <param name="text">The text to search for.</param>
        /// <param name="matchCase">Whether to match case.</param>
        /// <param name="wholeWord">Whether to match whole words only.</param>
        /// <param name="startPage">The page to start searching.</param>
        /// <param name="endPage">The page to end searching.</param>
        /// <returns>All matches.</returns>
        public PdfMatches Search(string text, bool matchCase, bool wholeWord, int startPage, int endPage)
        {
            return _file.Search(text, matchCase, wholeWord, startPage, endPage);
        }


        /// <summary>
        /// Get all text on the page.
        /// </summary>
        /// <param name="page">The page to get the text for.</param>
        /// <returns>The text on the page.</returns>
        public string GetPdfText(int page)
        {
            return _file.GetPdfText(page);
        }


        /// <summary>
        /// Get all text matching the text span.
        /// </summary>
        /// <param name="textSpan">The span to get the text for.</param>
        /// <returns>The text matching the span.</returns>
        public string GetPdfText(PdfTextSpan textSpan)
        {
            return _file.GetPdfText(textSpan);
        }



        /// <summary>
        /// Get the current rotation of the page.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public PdfRotation GetPageRotation(int page)
        {
            return _file.GetPageRotation(page);
        }


        /// <summary>
        /// Renders a single page of a given PDF into a SKImage.
        /// </summary>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        /// <returns>The rendered PDF page as a SKImage.</returns>
        public SKImage LoadImage(int page = 0, RenderOptions options = default)
        {

            if (page < 0)
                throw new ArgumentOutOfRangeException(nameof(page), "The page number must be 0 or greater.");

            if (options == default)
                options = new();


            NativeMethods.FPDF renderFlags = default;

            if (options.WithAnnotations)
                renderFlags |= NativeMethods.FPDF.ANNOT;

            if (!options.AntiAliasing.HasFlag(PdfAntiAliasing.Text))
                renderFlags |= NativeMethods.FPDF.RENDER_NO_SMOOTHTEXT;
            if (!options.AntiAliasing.HasFlag(PdfAntiAliasing.Images))
                renderFlags |= NativeMethods.FPDF.RENDER_NO_SMOOTHIMAGE;
            if (!options.AntiAliasing.HasFlag(PdfAntiAliasing.Paths))
                renderFlags |= NativeMethods.FPDF.RENDER_NO_SMOOTHPATH;


            if (page >= PageCount)
                throw new ArgumentOutOfRangeException(nameof(page), $"The page number {page} does not exist. Highest page number available is {PageCount - 1}.");

            return Render(page,
                          options.Width,
                          options.Height,
                          options.Dpi,
                          options.Rotation,
                          renderFlags,
                          options.WithFormFill,
                          options.BackgroundColor ?? Color4.White,
                          options.Bounds,
                          options.UseTiling,
                          options.WithAspectRatio,
                          options.DpiRelativeToBounds);
        }


        /// <summary>
        /// Renders a single page of a given PDF into a Stride Texture.
        /// </summary>
        /// <param name="device">A Stride.Graphics.Graphics.Device</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        /// <param name="textureFlags">Stride.Graphics.TextureFlags</param>
        /// <param name="usage">Stride.Graphics.GraphicsResourceUsage</param>
        /// <returns>The rendered PDF page as a Stride Texture.</returns>
        public Texture LoadTexture(GraphicsDevice device,
                                   int page = 0,
                                   RenderOptions options = default,
                                   TextureFlags textureFlags = TextureFlags.ShaderResource,
                                   GraphicsResourceUsage usage = GraphicsResourceUsage.Immutable)
        {

            if (page < 0)
                throw new ArgumentOutOfRangeException(nameof(page), "The page number must be 0 or greater.");

            if (options == default)
                options = new();


            NativeMethods.FPDF renderFlags = default;

            if (options.WithAnnotations)
                renderFlags |= NativeMethods.FPDF.ANNOT;

            if (!options.AntiAliasing.HasFlag(PdfAntiAliasing.Text))
                renderFlags |= NativeMethods.FPDF.RENDER_NO_SMOOTHTEXT;
            if (!options.AntiAliasing.HasFlag(PdfAntiAliasing.Images))
                renderFlags |= NativeMethods.FPDF.RENDER_NO_SMOOTHIMAGE;
            if (!options.AntiAliasing.HasFlag(PdfAntiAliasing.Paths))
                renderFlags |= NativeMethods.FPDF.RENDER_NO_SMOOTHPATH;


            if (page >= PageCount)
                throw new ArgumentOutOfRangeException(nameof(page), $"The page number {page} does not exist. Highest page number available is {PageCount - 1}.");


            return Render(page,
                          options.Width,
                          options.Height,
                          options.Dpi,
                          options.Rotation,
                          renderFlags,
                          options.WithFormFill,
                          options.BackgroundColor ?? Color4.White,
                          options.Bounds,
                          options.UseTiling,
                          options.WithAspectRatio,
                          options.DpiRelativeToBounds,
                          device,
                          textureFlags,
                          usage);
        }

        /// <summary>
        /// Renders a page of the PDF document to a texture.
        /// </summary>
        private Texture Render(int page,
                               float? requestedWidth,
                               float? requestedHeight,
                               float dpi,
                               PdfRotation rotate,
                               NativeMethods.FPDF flags,
                               bool renderFormFill,
                               Color4 backgroundColor,
                               RectangleF? bounds,
                               bool useTiling,
                               bool withAspectRatio,
                               bool dpiRelativeToBounds,
                               GraphicsDevice device,
                               TextureFlags textureFlags = TextureFlags.ShaderResource,
                               GraphicsResourceUsage usage = GraphicsResourceUsage.Immutable,
                               CancellationToken cancellationToken = default)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);


            SKColor bgcolor = Conversions.ToSKColor(ref backgroundColor);

            using var pixmap = Render(page,
                                      requestedWidth,
                                      requestedHeight,
                                      dpi,
                                      rotate,
                                      flags,
                                      renderFormFill,
                                      backgroundColor,
                                      bounds,
                                      useTiling,
                                      withAspectRatio,
                                      dpiRelativeToBounds,
                                      cancellationToken).PeekPixels();

            var description = TextureDescription.New2D(
                width: pixmap.Width,
                height: pixmap.Height,
                format: PixelFormat.B8G8R8A8_UNorm_SRgb,
                textureFlags: textureFlags,
                usage: usage);

            return Texture.New(device, description, new DataBox(pixmap.GetPixels(), pixmap.RowBytes, pixmap.BytesSize));

        }


        /// <summary>
        /// Renders a page of the PDF document to an image.
        /// </summary>
        private SKImage Render(int page,
                               float? requestedWidth,
                               float? requestedHeight,
                               float dpi,
                               PdfRotation rotate,
                               NativeMethods.FPDF flags,
                               bool renderFormFill,
                               Color4 backgroundColor,
                               RectangleF? bounds,
                               bool useTiling,
                               bool withAspectRatio,
                               bool dpiRelativeToBounds,
                               CancellationToken cancellationToken = default)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            // correct the width and height for the given dpi
            // but only if both width and height are not specified (so the original sizes are corrected)
            var correctFromDpi = requestedWidth == null && requestedHeight == null;

            var pagesize = PageSizes[page];

            var originalWidth = pagesize.X;
            var originalHeight = pagesize.Y;

            if (withAspectRatio && !(dpiRelativeToBounds && bounds.HasValue))
            {
                AdjustForAspectRatio(ref requestedWidth, ref requestedHeight, pagesize);
            }

            float width = requestedWidth ?? originalWidth;
            float height = requestedHeight ?? originalHeight;

            if (rotate == PdfRotation.Rotate90 || rotate == PdfRotation.Rotate270)
            {
                (width, height) = (height, width);
                (originalWidth, originalHeight) = (originalHeight, originalWidth);
            }

            if (correctFromDpi)
            {
                width *= dpi / 72f;
                height *= dpi / 72f;

                originalWidth *= dpi / 72f;
                originalHeight *= dpi / 72f;

                if (bounds != null)
                {
                    bounds = new RectangleF(
                        bounds.Value.X * (dpi / 72f),
                        bounds.Value.Y * (dpi / 72f),
                        bounds.Value.Width * (dpi / 72f),
                        bounds.Value.Height * (dpi / 72f)
                    );
                }
            }

            if (dpiRelativeToBounds && bounds.HasValue)
            {
                float? boundsWidth = requestedWidth != null ? requestedWidth : null;
                float? boundsHeight = requestedHeight != null ? requestedHeight : null;

                if (withAspectRatio)
                {
                    AdjustForAspectRatio(ref boundsWidth, ref boundsHeight, new Vector2(bounds.Value.Width, bounds.Value.Height));
                }

                var remainderX = 0f;
                var remainderY = 0f;

                if (requestedWidth == null)
                {
                    var newWidth = boundsWidth ?? bounds.Value.Width;
                    remainderX = 1 - (newWidth % 1);
                    width = (float)Math.Ceiling(newWidth);
                }

                if (requestedHeight == null)
                {
                    var newHeight = boundsHeight ?? bounds.Value.Height;
                    remainderY = 1 - (newHeight % 1);
                    height = (float)Math.Ceiling(newHeight);
                }

                bounds = new RectangleF(
                    bounds.Value.X * (width / originalWidth),
                    bounds.Value.Y * (height / originalHeight),
                    bounds.Value.Width + remainderX,
                    bounds.Value.Height + remainderY);

                remainderX = bounds.Value.X % 1;
                remainderY = bounds.Value.Y % 1;

                bounds = new RectangleF(
                    bounds.Value.X,
                    bounds.Value.Y,
                    bounds.Value.Width + remainderX,
                    bounds.Value.Height + remainderY);
            }

            if (bounds != null)
            {
                var factorX = width / originalWidth;
                var factorY = height / originalHeight;

                if (rotate == PdfRotation.Rotate90)
                {
                    bounds = new RectangleF(
                        ((originalWidth - bounds.Value.Height) * factorX) - bounds.Value.Y,
                        bounds.Value.X,
                        bounds.Value.Height,
                        bounds.Value.Width
                        );
                }
                else if (rotate == PdfRotation.Rotate270)
                {
                    bounds = new RectangleF(
                        bounds.Value.Y,
                        ((originalHeight - bounds.Value.Width) * factorY) - bounds.Value.X,
                        bounds.Value.Height,
                        bounds.Value.Width
                        );
                }
                else if (rotate == PdfRotation.Rotate180)
                {
                    bounds = new RectangleF(
                        ((originalWidth - bounds.Value.Width) * factorX) - bounds.Value.X,
                        ((originalHeight - bounds.Value.Height) * factorY) - bounds.Value.Y,
                        bounds.Value.Width,
                        bounds.Value.Height
                        );
                }
            }


            SKImage image;

            SKColor bgcolor = Conversions.ToSKColor(ref backgroundColor);

            int horizontalTileCount = (int)Math.Ceiling(width / _maxTileSize.X);
            int verticalTileCount = (int)Math.Ceiling(height / _maxTileSize.Y);

            if (!useTiling || (horizontalTileCount == 1 && verticalTileCount == 1))
            {
                image = RenderSubset(page, width, height, rotate, flags, renderFormFill, bgcolor, bounds, originalWidth, originalHeight, cancellationToken);
            }
            else
            {
                var info = new SKImageInfo((int)width, (int)height, SKColorType.Bgra8888, SKAlphaType.Premul);

                using (var surface = SKSurface.Create(info))
                {

                    cancellationToken.ThrowIfCancellationRequested();

                    float currentTileWidth = width / horizontalTileCount;
                    float currentTileHeight = height / verticalTileCount;
                    float boundsWidthFactor = bounds != null ? bounds.Value.Width / originalWidth : 0f;
                    float boundsHeightFactor = bounds != null ? bounds.Value.Height / originalHeight : 0f;

                    SKCanvas canvas = surface.Canvas;

                    canvas.Clear(bgcolor);

                    for (int y = 0; y < verticalTileCount; y++)
                    {
                        for (int x = 0; x < horizontalTileCount; x++)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            RectangleF currentBounds;

                            if (bounds != null)
                            {
                                currentBounds = new(
                                    (bounds.Value.X * (currentTileWidth / width)) + (currentTileWidth / horizontalTileCount * x * boundsWidthFactor),
                                    (bounds.Value.Y * (currentTileHeight / height)) + (currentTileHeight / verticalTileCount * y * boundsHeightFactor),
                                    currentTileWidth * boundsWidthFactor,
                                    currentTileHeight * boundsHeightFactor);
                            }
                            else
                            {
                                currentBounds = new(
                                    currentTileWidth / horizontalTileCount * x,
                                    currentTileHeight / verticalTileCount * y,
                                    currentTileWidth,
                                    currentTileHeight);
                            }

                            using var subsetImage = RenderSubset(page, currentTileWidth, currentTileHeight, rotate, flags, renderFormFill, bgcolor, currentBounds, width, height, cancellationToken);

                            cancellationToken.ThrowIfCancellationRequested();

                            canvas.DrawImage(subsetImage, new SKRect(
                                (float)Math.Floor(x * currentTileWidth),
                                (float)Math.Floor(y * currentTileHeight),
                                (float)Math.Floor(x * currentTileWidth + currentTileWidth),
                                (float)Math.Floor(y * currentTileHeight + currentTileHeight)));
                            canvas.Flush();
                        }
                    }

                    image = surface.Snapshot();
                }
            }

            return image;
        }


        private SKImage RenderSubset(int page,
                                     float width,
                                     float height,
                                     PdfRotation rotate,
                                     NativeMethods.FPDF flags,
                                     bool renderFormFill,
                                     SKColor backgroundColor,
                                     RectangleF? bounds,
                                     float originalWidth,
                                     float originalHeight,
                                     CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using SKColorSpace colorspace = SKColorSpace.CreateSrgb().ToLinearGamma();

            var iinfo = new SKImageInfo((int)width, (int)height, SKColorType.Bgra8888, SKAlphaType.Premul, colorspace);

            var image = SKImage.Create(iinfo);

            IntPtr handle = IntPtr.Zero;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                handle = NativeMethods.FPDFBitmap_CreateEx((int)width, (int)height, NativeMethods.FPDFBitmap.BGRA, image.PeekPixels().GetPixels(), (int)width * 4);

                cancellationToken.ThrowIfCancellationRequested();
                NativeMethods.FPDFBitmap_FillRect(handle, 0, 0, (int)width, (int)height, (uint)backgroundColor);

                cancellationToken.ThrowIfCancellationRequested();
                bool success = _file.RenderPDFPageToBitmap(
                    page,
                    handle,
                    bounds != null ? -(int)Math.Floor(bounds.Value.X * (originalWidth / bounds.Value.Width)) : 0,
                    bounds != null ? -(int)Math.Floor(bounds.Value.Y * (originalHeight / bounds.Value.Height)) : 0,
                    bounds != null ? (int)Math.Ceiling(originalWidth * (width / bounds.Value.Width)) : (int)Math.Ceiling(width),
                    bounds != null ? (int)Math.Ceiling(originalHeight * (height / bounds.Value.Height)) : (int)Math.Ceiling(height),
                    (int)rotate,
                    flags,
                    renderFormFill
                );

                if (!success)
                    throw new Win32Exception();
            }
            catch
            {
                image?.Dispose();
                throw;
            }
            finally
            {
                if (handle != IntPtr.Zero)
                    NativeMethods.FPDFBitmap_Destroy(handle);
            }

            return image;
        }


        private void AdjustForAspectRatio(ref float? width, ref float? height, Vector2 pageSize)
        {
            if (width == null && height != null)
            {
                width = pageSize.X / pageSize.Y * height.Value;
            }
            else if (width != null && height == null)
            {
                height = pageSize.Y / pageSize.Y * width.Value;
            }
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">Whether this method is called from <see cref="Dispose()"/>.</param>
        private void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _file.Dispose();

                _disposed = true;
            }
        }
    }
}