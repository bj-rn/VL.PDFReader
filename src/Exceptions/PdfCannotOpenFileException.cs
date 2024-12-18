﻿using static VL.PDFReader.Internals.NativeMethods;

namespace VL.PDFReader.Exceptions
{
    /// <summary>
    /// Thrown if the PDF file was not found or could not be opened.
    /// </summary>
    public class PdfCannotOpenFileException : PdfException
    {
        internal override FPDF_ERR Error => FPDF_ERR.FILE;

        /// <inheritdoc/>
        public PdfCannotOpenFileException() : base("File not found or could not be opened.") { }
    }
}