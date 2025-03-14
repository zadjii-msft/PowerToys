// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;

namespace Microsoft.CommandPalette.Extensions.Toolkit;

public class ThumbnailHelper
{
    private static readonly string[] ImageExtensions =
    [
        ".png",
        ".jpg",
        ".jpeg",
        ".gif",
        ".bmp",
        ".tiff",
        ".ico",
    ];

    public static Task<IRandomAccessStream?> GetThumbnail(string path)
    {
        var extension = Path.GetExtension(path).ToLower(CultureInfo.InvariantCulture);
        try
        {
            if (ImageExtensions.Contains(extension))
            {
                return GetImageThumbnailAsync(path);
            }
            else
            {
                // return GetFileIconStream(path);
                return Task.Run(() =>
                {
                    IRandomAccessStream? result = null;
                    var thread = new Thread(() =>
                    {
                        var t = GetFileIconStream(path);

                        // t.ConfigureAwait(false);
                        // t.Wait();
                        // result = t.IsCompletedSuccessfully ? t.Result : null;
                        result = t;
                    });
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                    thread.Join();
                    return result;
                });
            }
        }
        catch (Exception)
        {
        }

        return Task.FromResult<IRandomAccessStream?>(null);
    }

    private const uint SHGFIICON = 0x000000100;
    private const uint SHGFILARGEICON = 0x000000000;
    private const uint SHGFIUSEFILEATTRIBUTES = 0x000000010;
    private const uint FILEATTRIBUTENORMAL = 0x00000080;

    private static IRandomAccessStream? GetFileIconStream(string filePath)
    {
        var shinfo = default(NativeMethods.SHFILEINFO);
        var hr = NativeMethods.SHGetFileInfo(filePath, FILEATTRIBUTENORMAL, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFIICON | SHGFILARGEICON | SHGFIUSEFILEATTRIBUTES);

        if (hr == 0 || shinfo.hIcon == 0)
        {
            return null;
        }

        try
        {
            // using var icon = Icon.FromHandle(shinfo.hIcon).Clone();
            using var icon = (Icon)Icon.FromHandle(shinfo.hIcon).Clone();
            using var bitmap = icon.ToBitmap();
            var stream = new InMemoryRandomAccessStream();
            bitmap.Save(stream.AsStream(), System.Drawing.Imaging.ImageFormat.Png);
            stream.Seek(0);

            // using (var memoryStream = new MemoryStream())
            // {
            //    icon.ToBitmap().Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
            //    memoryStream.Position = 0;

            // using var outputStream = stream.GetOutputStreamAt(0);
            //    using var dataWriter = new DataWriter(outputStream);
            //    dataWriter.WriteBytes(memoryStream.ToArray());

            // // dataWriter.StoreAsync().GetAwaiter().GetResult();
            //    await dataWriter.StoreAsync();
            //    await dataWriter.FlushAsync();
            // }
            return stream;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load icon for {filePath}");
            Debug.WriteLine(ex.ToString());
        }
        finally
        {
            // Delete HIcon to avoid memory leaks
            _ = NativeMethods.DestroyIcon(shinfo.hIcon);
        }

        return null;
    }

    private static async Task<IRandomAccessStream?> GetImageThumbnailAsync(string filePath)
    {
        var file = await StorageFile.GetFileFromPathAsync(filePath);
        var thumbnail = await file.GetThumbnailAsync(ThumbnailMode.PicturesView);
        return thumbnail;
    }
}
