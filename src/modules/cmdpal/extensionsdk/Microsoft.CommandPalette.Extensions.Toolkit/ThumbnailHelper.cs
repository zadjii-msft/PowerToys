// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Windows.Storage.Streams;

namespace Microsoft.CommandPalette.Extensions.Toolkit;

public class ThumbnailHelper
{
    public static readonly string ProgramDirectory = Directory.GetParent(path: AppContext.BaseDirectory)?.ToString() ?? string.Empty;
    public static readonly int ThumbnailSize = 64;

    private static readonly string[] ImageExtensions =
    {
        ".png",
        ".jpg",
        ".jpeg",
        ".gif",
        ".bmp",
        ".tiff",
        ".ico",
    };

    [Flags]
    public enum ThumbnailOptions
    {
        RESIZETOFIT = 0x00,
        BiggerSizeOk = 0x01,
        InMemoryOnly = 0x02,
        IconOnly = 0x04,
        ThumbnailOnly = 0x08,
        InCacheOnly = 0x10,
    }

    public static IRandomAccessStream? GetThumbnail(string path)
    {
        IRandomAccessStream? stream = null;

        if (!Path.IsPathRooted(path))
        {
            path = Path.Combine(ProgramDirectory, "Images", Path.GetFileName(path));
        }

        if (Directory.Exists(path))
        {
            /* Directories can also have thumbnails instead of shell icons.
             * Generating thumbnails for a bunch of folders while scrolling through
             * results from Everything makes a big impact on performance and
             * Wox responsibility.
             * - Solution: just load the icon
             */
            stream = GetThumbnail(path, ThumbnailSize, ThumbnailSize, ThumbnailOptions.IconOnly);
        }
        else if (File.Exists(path))
        {
            // Using InvariantCulture since this is internal
            var extension = Path.GetExtension(path).ToLower(CultureInfo.InvariantCulture);
            if (ImageExtensions.Contains(extension))
            {
                stream = GetThumbnail(path, ThumbnailSize, ThumbnailSize, ThumbnailOptions.ThumbnailOnly);
            }
            else if (extension == ".pdf" && DoesPdfUseAcrobatAsProvider())
            {
                // The PDF thumbnail provider from Adobe Reader and Acrobat Pro lets crash PT Run with an Dispatcher exception. (https://github.com/microsoft/PowerToys/issues/18166)
                // To not run into the crash, we only request the icon of PDF files if the PDF thumbnail handler is set to Adobe Reader/Acrobat Pro.
                // Also don't get thumbnail if the GenerateThumbnailsFromFiles option is off.
                stream = GetThumbnail(path, ThumbnailSize, ThumbnailSize, ThumbnailOptions.IconOnly);
            }
            else
            {
                stream = GetThumbnail(path, ThumbnailSize, ThumbnailSize, ThumbnailOptions.RESIZETOFIT);
            }
        }

        return stream;
    }

    // Based on https://stackoverflow.com/questions/21751747/extract-thumbnail-for-any-file-in-windows
    private const string IShellItem2Guid = "7E9FB0D3-919F-4307-AB2E-9B1860310C93";

    internal enum HResult
    {
        Ok = 0x0000,
        False = 0x0001,
        InvalidArguments = unchecked((int)0x80070057),
        OutOfMemory = unchecked((int)0x8007000E),
        NoInterface = unchecked((int)0x80004002),
        Fail = unchecked((int)0x80004005),
        ExtractionFailed = unchecked((int)0x8004B200),
        ElementNotFound = unchecked((int)0x80070490),
        TypeElementNotFound = unchecked((int)0x8002802B),
        NoObject = unchecked((int)0x800401E5),
        Win32ErrorCanceled = 1223,
        Canceled = unchecked((int)0x800704C7),
        ResourceInUse = unchecked((int)0x800700AA),
        AccessDenied = unchecked((int)0x80030005),
    }

    [ComImport]
    [Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IShellItemImageFactory
    {
        [PreserveSig]
        HResult GetImage(
        [In, MarshalAs(UnmanagedType.Struct)] NativeSize size,
        [In] ThumbnailOptions flags,
        [Out] out IntPtr phbm);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeSize
    {
        private int width;
        private int height;

        public int Width
        {
            set { width = value; }
        }

        public int Height
        {
            set { height = value; }
        }
    }

    public static IRandomAccessStream GetThumbnail(string fileName, int width, int height, ThumbnailOptions options)
    {
        var hBitmap = IntPtr.Zero;

        if (Path.GetExtension(fileName).Equals(".lnk", StringComparison.OrdinalIgnoreCase))
        {
            hBitmap = ExtractIconToHBitmap(fileName);
        }
        else
        {
            hBitmap = GetHBitmap(Path.GetFullPath(fileName), width, height, options);
        }

        try
        {
            var bitmap = Image.FromHbitmap(hBitmap);
            var stream = new InMemoryRandomAccessStream();
            using (var outputStream = stream.GetOutputStreamAt(0))
            using (var dataWriter = new DataWriter(outputStream))
            {
                using (var memoryStream = new MemoryStream())
                {
                    bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                    var bytes = memoryStream.ToArray();

                    dataWriter.WriteBytes(bytes);
                    dataWriter.StoreAsync().GetAwaiter().GetResult();
                }
            }

            return stream;
        }
        finally
        {
            // delete HBitmap to avoid memory leaks
            NativeMethods.DeleteObject(hBitmap);
        }
    }

    private static IntPtr GetHBitmap(string fileName, int width, int height, ThumbnailOptions options)
    {
        var hBitmap = IntPtr.Zero;
        IShellItem? nativeShellItem = null;

        try
        {
            var shellItem2Guid = new Guid(IShellItem2Guid);
#pragma warning disable IL2050 // Correctness of COM interop cannot be guaranteed after trimming. Interfaces and interface members might be removed.
            var retCode = NativeMethods.SHCreateItemFromParsingName(fileName, IntPtr.Zero, ref shellItem2Guid, out nativeShellItem);
#pragma warning restore IL2050 // Correctness of COM interop cannot be guaranteed after trimming. Interfaces and interface members might be removed.

            if (retCode != 0 || nativeShellItem == null)
            {
                throw Marshal.GetExceptionForHR(retCode) ?? new InvalidOperationException("Unknown error occurred.");
            }

            var nativeSize = new NativeSize
            {
                Width = width,
                Height = height,
            };

            var hr = ((IShellItemImageFactory)nativeShellItem).GetImage(nativeSize, options, out hBitmap);

            // if extracting image thumbnail and failed, extract shell icon
            if (options == ThumbnailOptions.ThumbnailOnly && hr == HResult.ExtractionFailed)
            {
                hr = ((IShellItemImageFactory)nativeShellItem).GetImage(nativeSize, ThumbnailOptions.IconOnly, out hBitmap);
            }

            if (hr != HResult.Ok)
            {
                throw Marshal.GetExceptionForHR((int)hr) ?? new InvalidOperationException("Unknown error occurred.");
            }

            return hBitmap;
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            if (nativeShellItem != null)
            {
                Marshal.ReleaseComObject(nativeShellItem);
            }
        }
    }

    public static IntPtr ExtractIconToHBitmap(string fileName)
    {
        // Extracts the icon associated with the file
        using (var thumbnailIcon = Icon.ExtractAssociatedIcon(fileName))
        {
            // Convert to Bitmap
            using (var bitmap = thumbnailIcon?.ToBitmap())
            {
                if (bitmap != null)
                {
                    return bitmap.GetHbitmap();
                }

                return IntPtr.Zero;
            }
        }
    }

    private static bool logReportedAdobeReaderDetected; // Keep track if Adobe Reader detection has been logged yet.
    private static bool logReportedErrorInDetectingAdobeReader; // Keep track if we reported an exception while trying to detect Adobe Reader yet.
    private static bool adobeReaderDetectionLastResult; // The last result when Adobe Reader detection has read the registry.
    private static DateTime adobeReaderDetectionLastTime; // The last time when Adobe Reader detection has read the registry.

    // We have to evaluate this in real time to not crash, if the user switches to Adobe Reader/Acrobat Pro after starting PT Run.
    // Adobe registers its thumbnail handler always in "HKCR\Acrobat.Document.*\shellex\{BB2E617C-0920-11d1-9A0B-00C04FC2D6C1}".
    public static bool DoesPdfUseAcrobatAsProvider()
    {
        // If the last run is not more than five seconds ago use its result.
        // Doing this we minimize the amount of Registry queries, if more than one new PDF file is shown in the results.
        if ((DateTime.Now - adobeReaderDetectionLastTime).TotalSeconds < 5)
        {
            return adobeReaderDetectionLastResult;
        }

        // If cache condition is false, then query the registry.
        try
        {
            // First detect if there is a provider other than Adobe. For example PowerToys.
            // Generic GUIDs used by Explorer to identify the configured provider types: {E357FCCD-A995-4576-B01F-234630154E96} = File thumbnail, {BB2E617C-0920-11d1-9A0B-00C04FC2D6C1} = Image thumbnail.
            var key1 = Registry.ClassesRoot.OpenSubKey(".pdf\\shellex\\{E357FCCD-A995-4576-B01F-234630154E96}", false);
            var value1 = key1 != null ? (string?)key1.GetValue(string.Empty) : null;
            key1?.Close();
            var key2 = Registry.ClassesRoot.OpenSubKey(".pdf\\shellex\\{BB2E617C-0920-11d1-9A0B-00C04FC2D6C1}", false);
            var value2 = key2 != null ? (string?)key2.GetValue(string.Empty) : null;
            key2?.Close();
            if (!string.IsNullOrEmpty(value1) || !string.IsNullOrEmpty(value2))
            {
                // A provider other than Adobe is used. (For example the PowerToys PDF Thumbnail provider.)
                logReportedAdobeReaderDetected = false; // Reset log marker to report when Adobe is reused. (Example: Adobe -> Test PowerToys. -> Back to Adobe.)
                adobeReaderDetectionLastResult = false;
                adobeReaderDetectionLastTime = DateTime.Now;
                return false;
            }

            // Then detect if Adobe is the default PDF application.
            // The global config can be found under "HKCR\.pdf", but the "UserChoice" key under HKCU has precedence.
            var pdfKeyUser = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\.pdf\\UserChoice", false);
            var pdfAppUser = (string?)pdfKeyUser?.GetValue("ProgId");
            pdfKeyUser?.Close();
            var pdfKeyGlobal = Registry.ClassesRoot.OpenSubKey(".pdf", false);
            var pdfAppGlobal = (string?)pdfKeyGlobal?.GetValue(string.Empty);
            pdfKeyGlobal?.Close();
            var pdfApp = !string.IsNullOrEmpty(pdfAppUser) ? pdfAppUser : pdfAppGlobal; // User choice has precedence.
            if (string.IsNullOrEmpty(pdfApp) || !pdfApp.StartsWith("Acrobat.Document.", StringComparison.OrdinalIgnoreCase))
            {
                // Adobe is not used as PDF application.
                logReportedAdobeReaderDetected = false; // Reset log marker to report when Adobe is reused. (Example: Adobe -> Test PowerToys. -> Back to Adobe.)
                adobeReaderDetectionLastResult = false;
                adobeReaderDetectionLastTime = DateTime.Now;
                return false;
            }

            // Detect if the thumbnail handler from Adobe is disabled.
            var adobeAppKey = Registry.ClassesRoot.OpenSubKey(pdfApp + "\\shellex\\{BB2E617C-0920-11d1-9A0B-00C04FC2D6C1}", false);
            var adobeAppProvider = (string?)adobeAppKey?.GetValue(string.Empty);
            adobeAppKey?.Close();
            if (string.IsNullOrEmpty(adobeAppProvider))
            {
                // No Adobe handler.
                logReportedAdobeReaderDetected = false; // Reset log marker to report when Adobe is reused. (Example: Adobe -> Test PowerToys. -> Back to Adobe.)
                adobeReaderDetectionLastResult = false;
                adobeReaderDetectionLastTime = DateTime.Now;
                return false;
            }

            // Thumbnail handler from Adobe is enabled and used.
            if (!logReportedAdobeReaderDetected)
            {
                logReportedAdobeReaderDetected = true;
            }

            adobeReaderDetectionLastResult = true;
            adobeReaderDetectionLastTime = DateTime.Now;
            return true;
        }
        catch (Exception)
        {
            if (!logReportedErrorInDetectingAdobeReader)
            {
                logReportedErrorInDetectingAdobeReader = true;
            }

            // If we fail to detect it, we return that Adobe is used. Otherwise we could run into the Dispatcher crash.
            // (This only results in showing the icon instead of a thumbnail. It has no other functional impact.)
            return true;
        }
    }
}
