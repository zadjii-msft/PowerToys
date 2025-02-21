// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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

    public static async Task<IRandomAccessStream?> GetThumbnail(string path)
    {
        var extension = Path.GetExtension(path).ToLower(CultureInfo.InvariantCulture);
        try
        {
            if (ImageExtensions.Contains(extension))
            {
                // asdf
                return await GetImageThumbnailAsync(path);
            }
            else
            {
                // asdf asdf
                var t = GetFileIconStream(path);
                await t;
                return t.Exception != null ? null : t.Result;
            }
        }
        catch (Exception)
        {
        }

        return null;
    }

    private const uint SHGFIICON = 0x000000100;
    private const uint SHGFILARGEICON = 0x000000000;

    private static async Task<IRandomAccessStream?> GetFileIconStream(string filePath)
    {
        try
        {
            var shinfo = default(NativeMethods.SHFILEINFO);
            var hr = NativeMethods.SHGetFileInfo(filePath, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFIICON | SHGFILARGEICON);
            if (hr == 0 || shinfo.hIcon == 0)
            {
                return null;
            }

            using var icon = (Icon)Icon.FromHandle(shinfo.hIcon).Clone();
            var stream = new InMemoryRandomAccessStream();
            using (var memoryStream = new MemoryStream())
            {
                icon.ToBitmap().Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                memoryStream.Position = 0;

                using var outputStream = stream.GetOutputStreamAt(0);
                using var dataWriter = new DataWriter(outputStream);
                dataWriter.WriteBytes(memoryStream.ToArray());

                // var awaiter = dataWriter.StoreAsync().GetAwaiter();
                // awaiter.
                // awaiter.GetResult();
                await dataWriter.StoreAsync();
                await dataWriter.FlushAsync();
                return stream;
            }

            // return stream;
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
        catch (ObjectDisposedException)
        {
            return null;
        }
        catch (NullReferenceException)
        {
            return null;
        }
    }

    private static async Task<IRandomAccessStream> GetImageThumbnailAsync(string filePath)
    {
        var file = await StorageFile.GetFileFromPathAsync(filePath);
        var thumbnail = await file.GetThumbnailAsync(ThumbnailMode.PicturesView);
        return thumbnail;
    }
}
