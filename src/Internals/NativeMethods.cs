using Silk.NET.OpenGL;
using Stride.Rendering.Lights;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VL.PDFReader.Internals
{

#pragma warning disable CA2101 // Specify marshalling for P/Invoke string arguments
#pragma warning disable SYSLIB1054 // Use LibraryImportAttribute instead of DllImportAttribute to generate p/invoke marshalling code at compile time.

    internal static partial class NativeMethods
    {
        // Interned strings are cached over AppDomains. This means that when we
        // lock on this string, we actually lock over AppDomain's. The Pdfium
        // library is not thread safe, and this way of locking
        // guarantees that we don't access the Pdfium library from different
        // threads, even when there are multiple AppDomain's in play.
        private static readonly string LockString = string.Intern("e362349b-001d-4cb2-bf55-a71606a3e36f");

        public static void FPDF_InitLibrary()
        {
            lock (LockString)
            {
                Imports.FPDF_InitLibrary();
            }
        }

        public static void FPDF_DestroyLibrary()
        {
            lock (LockString)
            {
                Imports.FPDF_DestroyLibrary();
            }
        }

        public static void FPDF_CloseDocument(IntPtr document)
        {
            lock (LockString)
            {
                Imports.FPDF_CloseDocument(document);
            }
        }

        public static int FPDF_GetPageCount(IntPtr document)
        {
            lock (LockString)
            {
                return Imports.FPDF_GetPageCount(document);
            }
        }

        public static uint FPDF_GetDocPermissions(IntPtr document)
        {
            lock (LockString)
            {
                return Imports.FPDF_GetDocPermissions(document);
            }
        }

        public static IntPtr FPDFDOC_InitFormFillEnvironment(IntPtr document, FPDF_FORMFILLINFO formInfo)
        {
            lock (LockString)
            {
                return Imports.FPDFDOC_InitFormFillEnvironment(document, formInfo);
            }
        }

        public static void FPDF_SetFormFieldHighlightColor(IntPtr hHandle, int fieldType, uint color)
        {
            lock (LockString)
            {
                Imports.FPDF_SetFormFieldHighlightColor(hHandle, fieldType, color);
            }
        }

        public static void FPDF_SetFormFieldHighlightAlpha(IntPtr hHandle, byte alpha)
        {
            lock (LockString)
            {
                Imports.FPDF_SetFormFieldHighlightAlpha(hHandle, alpha);
            }
        }

        public static IntPtr FPDF_LoadPage(IntPtr document, int page_index)
        {
            lock (LockString)
            {
                return Imports.FPDF_LoadPage(document, page_index);
            }
        }

        public static IntPtr FPDFText_LoadPage(IntPtr page)
        {
            lock (LockString)
            {
                return Imports.FPDFText_LoadPage(page);
            }
        }

        public static void FORM_OnAfterLoadPage(IntPtr page, IntPtr _form)
        {
            lock (LockString)
            {
                Imports.FORM_OnAfterLoadPage(page, _form);
            }
        }

        public static double FPDF_GetPageWidth(IntPtr page)
        {
            lock (LockString)
            {
                return Imports.FPDF_GetPageWidth(page);
            }
        }

        public static double FPDF_GetPageHeight(IntPtr page)
        {
            lock (LockString)
            {
                return Imports.FPDF_GetPageHeight(page);
            }
        }

        public static void FORM_OnBeforeClosePage(IntPtr page, IntPtr _form)
        {
            lock (LockString)
            {
                Imports.FORM_OnBeforeClosePage(page, _form);
            }
        }

        public static void FPDFText_ClosePage(IntPtr text_page)
        {
            lock (LockString)
            {
                Imports.FPDFText_ClosePage(text_page);
            }
        }

        public static void FPDF_ClosePage(IntPtr page)
        {
            lock (LockString)
            {
                Imports.FPDF_ClosePage(page);
            }
        }

        public static void FPDF_RenderPageBitmap(IntPtr bitmap, IntPtr page, int start_x, int start_y, int size_x, int size_y, int rotate, FPDF flags)
        {
            lock (LockString)
            {
                Imports.FPDF_RenderPageBitmap(bitmap, page, start_x, start_y, size_x, size_y, rotate, flags);
            }
        }

        public static int FPDF_GetPageSizeByIndex(IntPtr document, int page_index, out double width, out double height)
        {
            lock (LockString)
            {
                return Imports.FPDF_GetPageSizeByIndex(document, page_index, out width, out height);
            }
        }

        public static IntPtr FPDFBitmap_CreateEx(int width, int height, FPDFBitmap format, IntPtr first_scan, int stride)
        {
            lock (LockString)
            {
                return Imports.FPDFBitmap_CreateEx(width, height, (int)format, first_scan, stride);
            }
        }

        public static void FPDFBitmap_FillRect(IntPtr bitmapHandle, int left, int top, int width, int height, uint color)
        {
            lock (LockString)
            {
                Imports.FPDFBitmap_FillRect(bitmapHandle, left, top, width, height, color);
            }
        }

        public static void FPDFBitmap_Destroy(IntPtr bitmapHandle)
        {
            lock (LockString)
            {
                Imports.FPDFBitmap_Destroy(bitmapHandle);
            }
        }

        public static uint FPDFDest_GetDestPageIndex(IntPtr document, IntPtr dest)
        {
            lock (LockString)
            {
                return Imports.FPDFDest_GetDestPageIndex(document, dest);
            }
        }


        public static IntPtr FPDFText_FindStart(IntPtr page, byte[] findWhat, FPDF_SEARCH_FLAGS flags, int start_index)
        {
            lock (LockString)
            {
                return Imports.FPDFText_FindStart(page, findWhat, flags, start_index);
            }
        }


        public static int FPDFText_CountChars(IntPtr page)
        {
            lock (LockString)
            {
                return Imports.FPDFText_CountChars(page);
            }
        }

        public static bool FPDFText_FindNext(IntPtr handle)
        {
            lock (LockString)
            {
                return Imports.FPDFText_FindNext(handle);
            }
        }

        public static void FPDFText_FindClose(IntPtr handle)
        {
            lock (LockString)
            {
                Imports.FPDFText_FindClose(handle);
            }
        }

        public static int FPDFText_GetSchResultIndex(IntPtr handle)
        {
            lock (LockString)
            {
                return Imports.FPDFText_GetSchResultIndex(handle);
            }
        }

        public static int FPDFText_GetSchCount(IntPtr handle)
        {
            lock (LockString)
            {
                return Imports.FPDFText_GetSchCount(handle);
            }
        }

        public static int FPDFText_GetText(IntPtr page, int start_index, int count, byte[] result)
        {
            lock (LockString)
            {
                return Imports.FPDFText_GetText(page, start_index, count, result);
            }
        }


        public static uint FPDF_GetMetaText(IntPtr document, string tag, byte[]? buffer, uint buflen)
        {
            lock (LockString)
            {
                return Imports.FPDF_GetMetaText(document, tag, buffer, buflen);
            }
        }


        public static PdfRotation FPDFPage_GetRotation(IntPtr page)
        {
            lock (LockString)
            {
                return (PdfRotation)Imports.FPDFPage_GetRotation(page);
            }
        }


        /// <summary>
        /// Opens a document using a .NET Stream. Allows opening huge
        /// PDFs without loading them into memory first.
        /// </summary>
        /// <param name="input">The input Stream. Don't dispose prior to closing the pdf.</param>
        /// <param name="password">Password, if the PDF is protected. Can be null.</param>
        /// <param name="id">Retrieves an IntPtr to the COM object for the Stream. The caller must release this with Marshal.Release prior to Disposing the Stream.</param>
        /// <returns>An IntPtr to the FPDF_DOCUMENT object.</returns>

        public static unsafe IntPtr FPDF_LoadCustomDocument(Stream input, string? password, int id)
        {
            delegate* unmanaged[Cdecl]<IntPtr, uint, IntPtr, uint, int> getBlock = &FPDF_GetBlock;

            var access = new FPDF_FILEACCESS
            {
                m_FileLen = (uint)input.Length,
                m_GetBlock = getBlock,
                m_Param = (IntPtr)id
            };

            lock (LockString)
            {
                return Imports.FPDF_LoadCustomDocument(access, password);
            }
        }

        public static FPDF_ERR FPDF_GetLastError()
        {
            lock (LockString)
            {
                return (FPDF_ERR)Imports.FPDF_GetLastError();
            }
        }

        public static void FPDF_FFLDraw(IntPtr form, IntPtr bitmap, IntPtr page, int start_x, int start_y, int size_x, int size_y, int rotate, FPDF flags)
        {
            lock (LockString)
            {
                Imports.FPDF_FFLDraw(form, bitmap, page, start_x, start_y, size_x, size_y, rotate, flags);
            }
        }

        public static void FPDF_RemoveFormFieldHighlight(IntPtr form)
        {
            lock (LockString)
            {
                Imports.FPDF_RemoveFormFieldHighlight(form);
            }
        }

        public static int FPDFText_GetBoundedText(IntPtr page, double left, double top, double right, double bottom, byte[]? buffer, int bufferSize)
        {
            lock (LockString)
            {
                return Imports.FPDFText_GetBoundedText(page, left, top, right, bottom, buffer, bufferSize);
            }
        }


        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        private static int FPDF_GetBlock(IntPtr param, uint position, IntPtr buffer, uint size)
        {
            var stream = StreamManager.Get(checked((int)param));
            if (stream == null)
                return 0;
            byte[] managedBuffer = new byte[size];

            stream.Position = position;
            int read = stream.Read(managedBuffer, 0, (int)size);
            if (read != size)
                return 0;

            Marshal.Copy(managedBuffer, 0, buffer, (int)size);
            return 1;
        }

        private static partial class Imports
        {

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial void FPDF_InitLibrary();

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial void FPDF_DestroyLibrary();

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial void FPDF_CloseDocument(IntPtr document);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial int FPDF_GetPageCount(IntPtr document);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial uint FPDF_GetDocPermissions(IntPtr document);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial void FPDF_SetFormFieldHighlightColor(IntPtr hHandle, int fieldType, uint color);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial void FPDF_SetFormFieldHighlightAlpha(IntPtr hHandle, byte alpha);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial IntPtr FPDF_LoadPage(IntPtr document, int page_index);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial IntPtr FPDFText_LoadPage(IntPtr page);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial void FORM_OnAfterLoadPage(IntPtr page, IntPtr _form);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial double FPDF_GetPageWidth(IntPtr page);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial double FPDF_GetPageHeight(IntPtr page);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial void FORM_OnBeforeClosePage(IntPtr page, IntPtr _form);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial void FPDFText_ClosePage(IntPtr text_page);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial void FPDF_ClosePage(IntPtr page);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial void FPDF_RenderPageBitmap(IntPtr bitmapHandle, IntPtr page, int start_x, int start_y, int size_x, int size_y, int rotate, FPDF flags);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
            public static partial int FPDF_GetPageSizeByIndex(IntPtr document, int page_index, out double width, out double height);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial IntPtr FPDFBitmap_CreateEx(int width, int height, int format, IntPtr first_scan, int stride);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial void FPDFBitmap_FillRect(IntPtr bitmapHandle, int left, int top, int width, int height, uint color);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial void FPDFBitmap_Destroy(IntPtr bitmapHandle);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial uint FPDFDest_GetDestPageIndex(IntPtr document, IntPtr dest);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial uint FPDF_GetLastError();

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial void FPDF_FFLDraw(IntPtr form, IntPtr bitmap, IntPtr page, int start_x, int start_y, int size_x, int size_y, int rotate, FPDF flags);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial void FPDF_RemoveFormFieldHighlight(IntPtr form);



            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial IntPtr FPDFText_FindStart(IntPtr page, byte[] findWhat, FPDF_SEARCH_FLAGS flags, int start_index);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial int FPDFText_CountChars(IntPtr page);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial void FPDFText_FindClose(IntPtr handle);


            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial int FPDFText_GetSchResultIndex(IntPtr handle);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial int FPDFText_GetSchCount(IntPtr handle);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial int FPDFText_GetText(IntPtr page, int start_index, int count, byte[] result);


            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial int FPDFPage_GetRotation(IntPtr page);




            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern bool FPDFText_FindNext(IntPtr handle);


            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern IntPtr FPDF_LoadCustomDocument(FPDF_FILEACCESS access, string? password);


            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr FPDFDOC_InitFormFillEnvironment(IntPtr document, FPDF_FORMFILLINFO formInfo);


            [DllImport("pdfium.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern uint FPDF_GetMetaText(IntPtr document, string tag, byte[]? buffer, uint buflen);

            [DllImport("pdfium.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern int FPDFText_GetBoundedText(IntPtr page, double left, double top, double right, double bottom, byte[]? buffer, int buflen);
        }



        [StructLayout(LayoutKind.Sequential)]
        public unsafe class FPDF_FILEACCESS
        {
            public uint m_FileLen;

            public delegate* unmanaged[Cdecl]<IntPtr, uint, IntPtr, uint, int> m_GetBlock;

            public IntPtr m_Param;
        }


        [StructLayout(LayoutKind.Sequential)]
        public class FPDF_FORMFILLINFO
        {
            public int version;

            private readonly IntPtr Release;

            private readonly IntPtr FFI_Invalidate;

            private readonly IntPtr FFI_OutputSelectedRect;

            private readonly IntPtr FFI_SetCursor;

            private readonly IntPtr FFI_SetTimer;

            private readonly IntPtr FFI_KillTimer;

            private readonly IntPtr FFI_GetLocalTime;

            private readonly IntPtr FFI_OnChange;

            private readonly IntPtr FFI_GetPage;

            private readonly IntPtr FFI_GetCurrentPage;

            private readonly IntPtr FFI_GetRotation;

            private readonly IntPtr FFI_ExecuteNamedAction;

            private readonly IntPtr FFI_SetTextFieldFocus;

            private readonly IntPtr FFI_DoURIAction;

            private readonly IntPtr FFI_DoGoToAction;

            private readonly IntPtr m_pJsPlatform;

            private readonly IntPtr FFI_DisplayCaret;

            private readonly IntPtr FFI_GetCurrentPageIndex;

            private readonly IntPtr FFI_SetCurrentPage;

            private readonly IntPtr FFI_GotoURL;

            private readonly IntPtr FFI_GetPageViewRect;

            private readonly IntPtr FFI_PageEvent;

            private readonly IntPtr FFI_PopupMenu;

            private readonly IntPtr FFI_OpenFile;

            private readonly IntPtr FFI_EmailTo;

            private readonly IntPtr FFI_UploadTo;

            private readonly IntPtr FFI_GetPlatform;

            private readonly IntPtr FFI_GetLanguage;

            private readonly IntPtr FFI_DownloadFromURL;

            private readonly IntPtr FFI_PostRequestURL;

            private readonly IntPtr FFI_PutRequestURL;
        }



        /// <summary>
        /// A number indicating for bitmap format.
        /// </summary>
        public enum FPDFBitmap : int
        {
            /// <summary>
            /// Gray scale bitmap, one byte per pixel.
            /// </summary>
            Gray = 1,

            /// <summary>
            /// 3 bytes per pixel, byte order: blue, green, red.
            /// </summary>
            BGR = 2,

            /// <summary>
            /// 4 bytes per pixel, byte order: blue, green, red, unused.
            /// </summary>
            BGRx = 3,

            /// <summary>
            /// 4 bytes per pixel, byte order: blue, green, red, alpha.
            /// </summary>
            BGRA = 4
        }

        [Flags]
        public enum FPDF
        {
            /// <summary>
            /// Set if annotations are to be rendered.
            /// </summary>
            ANNOT = 0x01,

            /// <summary>
            /// Set if using text rendering optimized for LCD display. This flag will only take effect if anti-aliasing is enabled for text.
            /// </summary>
            LCD_TEXT = 0x02,

            /// <summary>
            /// Don't use the native text output available on some platforms.
            /// </summary>
            NO_NATIVETEXT = 0x04,

            /// <summary>
            /// Grayscale output.
            /// </summary>
            GRAYSCALE = 0x08,

            /// <summary>
            /// Obsolete, has no effect, retained for compatibility.
            /// </summary>
            [Obsolete("Obsolete, has no effect, retained for compatibility.")]
            DEBUG_INFO = 0x80,

            /// <summary>
            /// Obsolete, has no effect, retained for compatibility.
            /// </summary>
            [Obsolete("Obsolete, has no effect, retained for compatibility.")]
            NO_CATCH = 0x100,

            /// <summary>
            /// Limit image cache size.
            /// </summary>
            RENDER_LIMITEDIMAGECACHE = 0x200,

            /// <summary>
            /// Always use halftone for image stretching.
            /// </summary>
            RENDER_FORCEHALFTONE = 0x400,

            /// <summary>
            /// Render for printing.
            /// </summary>
            PRINTING = 0x800,

            /// <summary>
            /// Set to disable anti-aliasing on text. This flag will also disable LCD optimization for text rendering.
            /// </summary>
            RENDER_NO_SMOOTHTEXT = 0x1000,

            /// <summary>
            /// Set to disable anti-aliasing on images.
            /// </summary>
            RENDER_NO_SMOOTHIMAGE = 0x2000,

            /// <summary>
            /// Set to disable anti-aliasing on paths.
            /// </summary>
            RENDER_NO_SMOOTHPATH = 0x4000,

            /// <summary>
            /// Set whether to render in a reverse Byte order, this flag is only used when rendering to a bitmap.
            /// </summary>
            REVERSE_BYTE_ORDER = 0x10,

            /// <summary>
            /// Set whether fill paths need to be stroked. This flag is only used when FPDF_COLORSCHEME is passed in, since with a single fill color for paths the boundaries of adjacent fill paths are less visible.
            /// </summary>
            CONVERT_FILL_TO_STROKE = 0x20
        }

        public enum FPDF_ERR : uint
        {
            /// <summary>
            /// No error.
            /// </summary>
            SUCCESS = 0,

            /// <summary>
            /// Unknown error.
            /// </summary>
            UNKNOWN = 1,

            /// <summary>
            /// File not found or could not be opened.
            /// </summary>
            FILE = 2,

            /// <summary>
            /// File not in PDF format or corrupted.
            /// </summary>
            FORMAT = 3,

            /// <summary>
            /// Password required or incorrect password.
            /// </summary>
            PASSWORD = 4,

            /// <summary>
            /// Unsupported security scheme.
            /// </summary>
            SECURITY = 5,

            /// <summary>
            /// Page not found or content error.
            /// </summary>
            PAGE = 6
        }


        public enum FPDF_SEARCH_FLAGS
        {
            FPDF_MATCHCASE = 1,
            FPDF_MATCHWHOLEWORD = 2
        }

    }
}