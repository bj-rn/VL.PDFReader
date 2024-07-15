using VL.PDFReader.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Stride.Core.Mathematics;

namespace VL.PDFReader.Internals
{

#pragma warning disable CA1513 // Use ObjectDisposedException throw helper
#pragma warning disable IDE0056 // Use index operator
#pragma warning disable IDE0057 // Use range operator

    internal class PdfFile : IDisposable
    {
        private static readonly Encoding FPDFEncoding = new UnicodeEncoding(false, false, false);

        private IntPtr _document;
        private IntPtr _form;
        private bool _disposed;
        private GCHandle _formCallbacksHandle;
        private readonly int _id;
        private Stream? _stream;
        private readonly bool _disposeStream;

        public PdfFile(Stream stream, string? password, bool disposeStream)
        {
            PdfLibrary.EnsureLoaded();

            _stream = stream ?? throw new ArgumentNullException(nameof(stream));

            try
            {
                // test if the given stream is seekable by getting its length
                var _ = _stream.Length;
            }
            catch (NotSupportedException ex)
            {
                if (!_stream.CanSeek)
                    throw new ArgumentException("The given stream does not support seeking.", nameof(stream), ex);

                throw;
            }

            _id = StreamManager.Register(stream);
            _disposeStream = disposeStream;

            var document = NativeMethods.FPDF_LoadCustomDocument(stream, password, _id);
            if (document == IntPtr.Zero)
                throw PdfException.CreateException(NativeMethods.FPDF_GetLastError())!;

            LoadDocument(document);
            _disposeStream = disposeStream;
        }

        private void LoadDocument(IntPtr document)
        {
            _document = document;

            NativeMethods.FPDF_GetDocPermissions(_document);

            var formCallbacks = new NativeMethods.FPDF_FORMFILLINFO();
            _formCallbacksHandle = GCHandle.Alloc(formCallbacks, GCHandleType.Pinned);

            // Depending on whether XFA support is built into the PDFium library, the version
            // needs to be 1 or 2. We don't really care, so we just try one or the other.

            for (int i = 1; i <= 2; i++)
            {
                formCallbacks.version = i;

                _form = NativeMethods.FPDFDOC_InitFormFillEnvironment(_document, formCallbacks);
                if (_form != IntPtr.Zero)
                    break;
            }

            NativeMethods.FPDF_SetFormFieldHighlightColor(_form, 0, 0xFFE4DD);
            NativeMethods.FPDF_SetFormFieldHighlightAlpha(_form, 100);
        }


        public List<Vector2> GetPDFDocInfo()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            int pageCount = NativeMethods.FPDF_GetPageCount(_document);
            var result = new List<Vector2>(pageCount);

            for (int i = 0; i < pageCount; i++)
            {
                result.Add(GetPDFDocInfo(i));
            }

            return result;
        }

        public Vector2 GetPDFDocInfo(int pageNumber)
        {
            NativeMethods.FPDF_GetPageSizeByIndex(_document, pageNumber, out double width, out double height);

            return new Vector2((float)width, (float)height);
        }


        public string GetPdfText(int page)
        {

            using (PageData pageData = GetPageData(page))
            {
                int length = NativeMethods.FPDFText_CountChars(pageData.TextPage);
                return GetPdfText(pageData, new PdfTextSpan(page, 0, length));
            }
        }

        public string GetPdfText(PdfTextSpan textSpan)
        {
            using (PageData pageData = GetPageData(textSpan.Page))
            {
                return GetPdfText(pageData, textSpan);
            }
        }

        private string GetPdfText(PageData pageData, PdfTextSpan textSpan)
        {
            // NOTE: The count parameter in FPDFText_GetText seems to include the null terminator, even though the documentation does not specify this.
            // So to read 40 characters, we need to allocate 82 bytes (2 for the terminator), and request 41 characters from GetText.
            // The return value also includes the terminator (which is documented)
            var result = new byte[(textSpan.Length + 1) * 2];
            int count = NativeMethods.FPDFText_GetText(pageData.TextPage, textSpan.Offset, textSpan.Length + 1, result);
            if (count <= 0)
                return string.Empty;
            return FPDFEncoding.GetString(result, 0, (count - 1) * 2);
        }


        public PdfMatches Search(string text, bool matchCase, bool wholeWord, int startPage, int endPage)
        {
            var matches = new List<PdfMatch>();

            if (String.IsNullOrEmpty(text))
                return new PdfMatches(startPage, endPage, matches);

            for (int page = startPage; page <= endPage; page++)
            {
                using var pageData = GetPageData(page);

                NativeMethods.FPDF_SEARCH_FLAGS flags = 0;
                if (matchCase)
                    flags |= NativeMethods.FPDF_SEARCH_FLAGS.FPDF_MATCHCASE;
                if (wholeWord)
                    flags |= NativeMethods.FPDF_SEARCH_FLAGS.FPDF_MATCHWHOLEWORD;

                var handle = NativeMethods.FPDFText_FindStart(pageData.TextPage, FPDFEncoding.GetBytes(text), flags, 0);

                try
                {
                    while (NativeMethods.FPDFText_FindNext(handle))
                    {
                        int index = NativeMethods.FPDFText_GetSchResultIndex(handle);

                        int matchLength = NativeMethods.FPDFText_GetSchCount(handle);

                        var result = new byte[(matchLength + 1) * 2];
                        NativeMethods.FPDFText_GetText(pageData.TextPage, index, matchLength, result);
                        string match = FPDFEncoding.GetString(result, 0, matchLength * 2);

                        matches.Add(new PdfMatch(
                            match,
                            new PdfTextSpan(page, index, matchLength),
                            page
                        ));
                    }
                }
                finally
                {
                    NativeMethods.FPDFText_FindClose(handle);
                }
            }

            return new PdfMatches(startPage, endPage, matches);
        }


        public bool RenderPDFPageToBitmap(int pageNumber, IntPtr bitmapHandle, int boundsOriginX, int boundsOriginY, int boundsWidth, int boundsHeight, int rotate, NativeMethods.FPDF flags, bool renderFormFill)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            using var pageData = GetPageData(pageNumber);

            NativeMethods.FPDF_RenderPageBitmap(bitmapHandle, pageData.Page, boundsOriginX, boundsOriginY, boundsWidth, boundsHeight, rotate, flags);

            if (renderFormFill)
            {
                NativeMethods.FPDF_RemoveFormFieldHighlight(_form);
                NativeMethods.FPDF_FFLDraw(_form, bitmapHandle, pageData.Page, boundsOriginX, boundsOriginY, boundsWidth, boundsHeight, rotate, flags);
            }

            return true;
        }


        private PageData GetPageData(int pageNumber)
        {
            return new PageData(_document, _form, pageNumber);
        }



        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            StreamManager.Unregister(_id);


            if (_form != IntPtr.Zero)
            {
                // this call is needed for PDFium builds with PDF_ENABLE_XFA enabled
                // and PDF_ENABLE_XFA implies JS support (PDF_ENABLE_V8 is needed for that)
                // since we don't ship PDFium with V8 we can skip this
                // otherwise we have to deal with some nasty unmanaged memory corruption
                //NativeMethods.FPDFDOC_ExitFormFillEnvironment(_form);
                _form = IntPtr.Zero;
            }

            if (_document != IntPtr.Zero)
            {
                NativeMethods.FPDF_CloseDocument(_document);
                _document = IntPtr.Zero;
            }

            if (_formCallbacksHandle!.IsAllocated)
                _formCallbacksHandle!.Free();

            if (_stream != null && _disposeStream)
            {
                _stream.Dispose();
                _stream = null;
            }

            _disposed = true;
        }

        private sealed class PageData : IDisposable
        {
            private readonly IntPtr _form;
            private bool _disposed;

            public IntPtr Page { get; private set; }

            public IntPtr TextPage { get; private set; }

            public double Width { get; private set; }

            public double Height { get; private set; }

            public PageData(IntPtr document, IntPtr form, int pageNumber)
            {
                _form = form;

                Page = NativeMethods.FPDF_LoadPage(document, pageNumber);
                TextPage = NativeMethods.FPDFText_LoadPage(Page);
                NativeMethods.FORM_OnAfterLoadPage(Page, form);

                Width = NativeMethods.FPDF_GetPageWidth(Page);
                Height = NativeMethods.FPDF_GetPageHeight(Page);
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    NativeMethods.FORM_OnBeforeClosePage(Page, _form);
                    NativeMethods.FPDFText_ClosePage(TextPage);
                    NativeMethods.FPDF_ClosePage(Page);

                    _disposed = true;
                }
            }
        }
    }
}