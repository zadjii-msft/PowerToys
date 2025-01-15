// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.CmdPal.Ext.WindowWalker.Commands;
using Microsoft.CmdPal.Ext.WindowWalker.Components;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Windows.Storage.Streams;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Microsoft.CmdPal.Ext.WindowWalker;

internal sealed partial class WindowWalkerListItem : ListItem
{
    private readonly Window? _window;

    public Window? Window => _window;

    private readonly IRandomAccessStream? _screenshot;

    public WindowWalkerListItem(Window? window)
        : base(new SwitchToWindowCommand(window))
    {
        _window = window;

        if (window != null)
        {
            try
            {
                _screenshot = CaptureWindowAsRandomAccessStream((HWND)window.Hwnd, window.Title);
                var reference = RandomAccessStreamReference.CreateFromStream(_screenshot);
                var iconData = IconData.FromStream(reference);
                var iconInfo = new IconInfo(iconData, iconData);
                Details = new Details() { HeroImage = iconInfo, Title = window.Title };
            }
            catch
            {
            }
        }
    }

    public static IRandomAccessStream CaptureWindowAsRandomAccessStream(HWND hWnd, string title)
    {
        // Get the window rectangle
        if (!PInvoke.GetWindowRect(hWnd, out var rect))
        {
            throw new InvalidOperationException("Failed to get window rectangle.");
        }

        var width = rect.right - rect.left;
        var height = rect.bottom - rect.top;

        // Create a bitmap and graphics object
        using (var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb))
        {
            using (var graphics = System.Drawing.Graphics.FromImage(bitmap)!)
            {
                var hdc = new HDC(graphics.GetHdc());
                var windowDC = PInvoke.GetDC(hWnd);

                // Copy the content from the window to the bitmap
                try
                {
                    bool success = PInvoke.BitBlt(hdc, 0, 0, width, height, windowDC, 0, 0, ROP_CODE.SRCCOPY);
                    if (!success)
                    {
                        var err = Marshal.GetLastWin32Error();
                        Debug.WriteLine(err);
                        throw new InvalidOperationException("Failed to capture window content.");
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    throw;
                }

                graphics.ReleaseHdc(hdc);
                var result = PInvoke.ReleaseDC(hWnd, windowDC);

                // if (!success)
                // {
                //    throw new InvalidOperationException("Failed to capture window content.");
                // }
            }

            //// Save bitmap to a memory stream
            // var path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            // path = Directory.GetParent(path)!.FullName;
            // var sanitized = title.Replace('\\', '_').Replace('/', '_').Replace('.', '_').Replace(' ', '_');
            // var full = $"{path}\\{sanitized}.png";
            // bitmap.Save(full, ImageFormat.Png);
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, ImageFormat.Png);
                memoryStream.Position = 0;

                // Convert MemoryStream to IRandomAccessStream
                return ConvertToRandomAccessStream(memoryStream);
            }
        }
    }

    private static IRandomAccessStream ConvertToRandomAccessStream(Stream stream)
    {
        var randomAccessStream = new InMemoryRandomAccessStream();
        using (var writer = new DataWriter(randomAccessStream.GetOutputStreamAt(0)))
        {
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            writer.WriteBytes(buffer);

            writer.StoreAsync().AsTask().Wait();
            writer.FlushAsync().AsTask().Wait();
        }

        return randomAccessStream;
    }
}
