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
        private PdfFile? _file;


        /// <summary>
        /// Size of each page in the PDF document.
        /// </summary>
        public IList<Vector2> PageSizes { get; private set; }


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
        /// <param name="disposeStream">Decides if the stream  <paramref name="path"/> will closed on dispose as well.</param>
        public static PdfDocument Load(Path path, string? password, bool disposeStream = true)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            return Load(File.OpenRead(path), password, disposeStream);
        }


        /// <summary>
        /// Initializes a new instance of the PdfDocument class with the provided stream.
        /// </summary>
        /// <param name="stream">Stream for the PDF document.</param>
        /// <param name="password">Password for the PDF document.</param>
        /// <param name="disposeStream">Decides if <paramref name="stream"/> will closed on dispose as well.</param>
        public static PdfDocument Load(Stream stream, string? password, bool disposeStream = true)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            return new PdfDocument(stream, password, disposeStream);
        }

        private PdfDocument(Stream stream, string? password, bool disposeStream)
        {
            _file = new PdfFile(stream, password, disposeStream);

            PageSizes = new ReadOnlyCollection<Vector2>(_file.GetPDFDocInfo() ?? throw new Win32Exception());
        }

        private const int MaxTileWidth = 4000;
        private const int MaxTileHeight = 4000;

        /// <summary>
        /// Renders a page of the PDF document to an image.
        /// </summary>
        /// <param name="page">Number of the page to render.</param>
        /// <param name="width">Width of the rendered image.</param>
        /// <param name="height">Height of the rendered image.</param>
        /// <param name="dpiX">Horizontal DPI.</param>
        /// <param name="dpiY">Vertical DPI.</param>
        /// <param name="rotate">Rotation.</param>
        /// <param name="flags">Flags used to influence the rendering.</param>
        /// <param name="renderFormFill">Render form fills.</param>
        /// <param name="correctFromDpi">Change <paramref name="width"/> and <paramref name="height"/> depending on the given <paramref name="dpiX"/> and <paramref name="dpiY"/>.</param>
        /// <param name="backgroundColor">The background color used for the output.</param>
        /// <param name="bounds">Specifies the bounds for the page relative to <see cref="Conversion.GetPageSizes(string,string)"/>. This can be used for clipping (bounds inside of page) or additional margins (bounds outside of page).</param>
        /// <param name="useTiling"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The rendered image.</returns>
        private SKImage Render(int page, float width, float height, float dpiX, float dpiY, PdfRotation rotate, NativeMethods.FPDF flags, bool renderFormFill, bool correctFromDpi, Color4 backgroundColor, RectangleF? bounds, bool useTiling, CancellationToken cancellationToken = default)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            var originalWidth = PageSizes[page].X;
            var originalHeight = PageSizes[page].Y;

            if (rotate == PdfRotation.Rotate90 || rotate == PdfRotation.Rotate270)
            {
                (width, height) = (height, width);
                (originalWidth, originalHeight) = (originalHeight, originalWidth);
                (dpiX, dpiY) = (dpiY, dpiX);
            }

            if (correctFromDpi)
            {
                width *= dpiX / 72f;
                height *= dpiY / 72f;

                originalWidth *= dpiX / 72f;
                originalHeight *= dpiY / 72f;

                if (bounds != null)
                {
                    bounds = new RectangleF(
                        bounds.Value.X * (dpiX / 72f),
                        bounds.Value.Y * (dpiY / 72f),
                        bounds.Value.Width * (dpiX / 72f),
                        bounds.Value.Height * (dpiY / 72f)
                    );
                }
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

            int horizontalTileCount = (int)Math.Ceiling(width / MaxTileWidth);
            int verticalTileCount = (int)Math.Ceiling(height / MaxTileHeight);

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


        private Texture Render (int page, float width, float height, float dpiX, float dpiY, PdfRotation rotate, NativeMethods.FPDF flags, bool renderFormFill, bool correctFromDpi, Color4 backgroundColor, RectangleF? bounds, bool useTiling, GraphicsDevice device, TextureFlags textureFlags = TextureFlags.ShaderResource, GraphicsResourceUsage usage = GraphicsResourceUsage.Immutable, CancellationToken cancellationToken = default)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);


            SKColor bgcolor = Conversions.ToSKColor(ref backgroundColor);
           
            using var pixmap = Render(page, width, height, dpiX, dpiY, rotate, flags, renderFormFill, correctFromDpi, backgroundColor, bounds, useTiling, cancellationToken).PeekPixels();

            var description = TextureDescription.New2D(
                width: pixmap.Width,
                height: pixmap.Height,
                format: PixelFormat.B8G8R8A8_UNorm_SRgb,
                textureFlags: textureFlags,
                usage: usage);

            return Texture.New(
                   device,
                   description,
                   new DataBox(pixmap.GetPixels(), pixmap.RowBytes, pixmap.BytesSize)); 

        }   


        private SKImage RenderSubset(int page, float width, float height, PdfRotation rotate, NativeMethods.FPDF flags, bool renderFormFill, SKColor backgroundColor, RectangleF? bounds, float originalWidth, float originalHeight, CancellationToken cancellationToken = default)
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
                bool success = _file!.RenderPDFPageToBitmap(
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



        public SKImage LoadImage(int page = 0, RenderOptions options = default)
        {

            if (page < 0)
                throw new ArgumentOutOfRangeException(nameof(page), "The page number must be 0 or greater.");

            if (options == default)
                options = new();

            // correct the width and height for the given dpi
            // but only if both width and height are not specified (so the original sizes are corrected)
            var correctFromDpi = options.Width == null && options.Height == null;

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

            var currentWidth = (float?)options.Width;
            var currentHeight = (float?)options.Height;
            var pageSize = this.PageSizes[page];

            // correct aspect ratio if requested
            if (options.WithAspectRatio)
                AdjustForAspectRatio(ref currentWidth, ref currentHeight, pageSize);

            return Render(page, currentWidth ?? pageSize.X, currentHeight ?? pageSize.Y, options.Dpi, options.Dpi, options.Rotation, renderFlags, options.WithFormFill, correctFromDpi, options.BackgroundColor ?? Color4.White, options.Bounds, options.UseTiling);
        }


        public Texture LoadTexture(GraphicsDevice device, int page = 0, RenderOptions options = default, TextureFlags textureFlags = TextureFlags.ShaderResource, GraphicsResourceUsage usage = GraphicsResourceUsage.Immutable)
        {

            if (page < 0)
                throw new ArgumentOutOfRangeException(nameof(page), "The page number must be 0 or greater.");

            if (options == default)
                options = new();

            // correct the width and height for the given dpi
            // but only if both width and height are not specified (so the original sizes are corrected)
            var correctFromDpi = options.Width == null && options.Height == null;

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

            var currentWidth = (float?)options.Width;
            var currentHeight = (float?)options.Height;
            var pageSize = this.PageSizes[page];

            // correct aspect ratio if requested
            if (options.WithAspectRatio)
                AdjustForAspectRatio(ref currentWidth, ref currentHeight, pageSize);

            // Internals.PdfDocument -> Image
            return Render(page, currentWidth ?? pageSize.X, currentHeight ?? pageSize.Y, options.Dpi, options.Dpi, options.Rotation, renderFlags, options.WithFormFill, correctFromDpi, options.BackgroundColor ?? Color4.White, options.Bounds, options.UseTiling, device, textureFlags, usage);
        }


        public  IEnumerable<SKImage> LoadAllImages(string? password = null, RenderOptions options = default)
        {

            if (options == default)
                options = new();

            // correct the width and height for the given dpi
            // but only if both width and height are not specified (so the original sizes are corrected)
            var correctFromDpi = options.Width == null && options.Height == null;

            NativeMethods.FPDF renderFlags = default;

            if (options.WithAnnotations)
                renderFlags |= NativeMethods.FPDF.ANNOT;

            if (!options.AntiAliasing.HasFlag(PdfAntiAliasing.Text))
                renderFlags |= NativeMethods.FPDF.RENDER_NO_SMOOTHTEXT;
            if (!options.AntiAliasing.HasFlag(PdfAntiAliasing.Images))
                renderFlags |= NativeMethods.FPDF.RENDER_NO_SMOOTHIMAGE;
            if (!options.AntiAliasing.HasFlag(PdfAntiAliasing.Paths))
                renderFlags |= NativeMethods.FPDF.RENDER_NO_SMOOTHPATH;


            for (int i = 0; i < PageCount; i++)
            {
                var currentWidth = (float?)options.Width;
                var currentHeight = (float?)options.Height;
                var pageSize = PageSizes[i];

                // correct aspect ratio if requested
                if (options.WithAspectRatio)
                    AdjustForAspectRatio(ref currentWidth, ref currentHeight, pageSize);

                yield return Render(i, currentWidth ?? pageSize.X, currentHeight ?? pageSize.Y, options.Dpi, options.Dpi, options.Rotation, renderFlags, options.WithFormFill, correctFromDpi, options.BackgroundColor ?? Color4.White, options.Bounds, options.UseTiling);
            }
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
                _file?.Dispose();
                _file = null;

                _disposed = true;
            }
        }
    }
}