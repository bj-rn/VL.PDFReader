using Stride.Core.Mathematics;

namespace VL.PDFReader
{
    /// <summary>
    /// Contains all relevant information to render a PDF page into an image.
    /// </summary>
    /// <param name="Dpi">The DPI scaling to use for rasterization of the PDF.</param>
    /// <param name="Width">The desired width of the page. Use <see langword="null"/> if the original width should be used.</param>
    /// <param name="Height">The desired height of the page. Use <see langword="null"/> if the original height should be used.</param>
    /// <param name="WithAnnotations">Specifies whether annotations will be rendered.</param>
    /// <param name="WithFormFill">Specifies whether form filling will be rendered.</param>
    /// <param name="WithAspectRatio">Specifies that <paramref name="Width"/> or <paramref name="Height"/> should be adjusted for aspect ratio (either one must be <see langword="null"/>).</param>
    /// <param name="Rotation">Specifies the rotation in 90 degree increments.</param>
    /// <param name="AntiAliasing">Specifies which elements of the PDF should be anti-aliased for rendering.</param>
    /// <param name="BackgroundColor">Specifies the background color. Defaults to <see cref="Color4.White"/>.</param>
    /// <param name="Bounds">Can be used for clipping (bounds inside of page) or additional margins (bounds outside of page) The bounds are relative to the PDF size (at 72 DPI).</param>
    /// <param name="DpiRelativeToBounds">Specifies wether <see cref="Dpi"/> and <see cref="WithAspectRatio"/> will be calculated relative to <see cref="Bounds"/> instead of the original PDF size (at 72 DPI).</param>
    /// <param name="UseTiling">Specifies wether the PDF should be rendered as several segments and merged into the final image. This can help in cases where the output image is too large, causing corrupted images (e.g. missing text) or crashes.</param>
    public readonly record struct RenderOptions(
        int Dpi = 72,
        int? Width = null,
        int? Height = null,
        bool WithAnnotations = true,
        bool WithFormFill = true,
        bool WithAspectRatio = false,
        PdfRotation Rotation = PdfRotation.Rotate0,
        PdfAntiAliasing AntiAliasing = PdfAntiAliasing.All,
        Color4? BackgroundColor = null,
        RectangleF? Bounds = null,
        bool DpiRelativeToBounds = false,
        bool UseTiling = false) : IRenderOptions
    {
        /// <summary>
        /// Constructs <see cref="RenderOptions"/> with default values.
        /// </summary>
        public RenderOptions() : this(72, null, null, true, true, false, PdfRotation.Rotate0, PdfAntiAliasing.All, null, null, false, false) { }
    }
}