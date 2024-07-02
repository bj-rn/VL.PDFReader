using static VL.PDFReader.Internals.NativeMethods;

namespace VL.PDFReader.Exceptions
{
    /// <summary>
    /// Thrown on unknown PDF errors.
    /// </summary>
    public class PdfUnknownException : PdfException
    {
        internal override FPDF_ERR Error => FPDF_ERR.UNKNOWN;

        /// <inheritdoc/>
        public PdfUnknownException() : base("Unknown error.") { }
    }
}