// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.CmdPal.Ext.WindowWalker;
using Microsoft.Graphics.Canvas;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Storage.Streams;

// Utility class for capturing windows
internal sealed class GraphicsCaptureHelper
{
    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public static async Task<IRandomAccessStream> CaptureWindowAsRandomAccessStreamAsync(IntPtr hwnd)
    {
        if (!GraphicsCaptureSession.IsSupported())
        {
            throw new NotSupportedException("Graphics Capture API is not supported on this system.");
        }

        // Create the GraphicsCaptureItem for the window
        var captureItem = CreateCaptureItemForWindow(hwnd);
        if (captureItem == null)
        {
            throw new InvalidOperationException("Failed to create capture item.");
        }

        // Use the Win2D CanvasDevice
        using var canvasDevice = new CanvasDevice();

        // Create a Direct3D11CaptureFramePool
        using var framePool = Direct3D11CaptureFramePool.Create(
            canvasDevice,
            DirectXPixelFormat.B8G8R8A8UIntNormalized,
            2,
            captureItem.Size);

        using var captureSession = framePool.CreateCaptureSession(captureItem);

        // captureSession.IsCursorCaptureEnabled = true;

        // Start the capture
        var completionSource = new TaskCompletionSource<Direct3D11CaptureFrame>();
        framePool.FrameArrived += (sender, args) =>
        {
            using var frame = sender.TryGetNextFrame();
            completionSource.TrySetResult(frame);
        };

        captureSession.StartCapture();

        // Wait for a frame to be captured
        var capturedFrame = await completionSource.Task;

        // Convert the captured frame to an IRandomAccessStream
        return await SaveFrameToRandomAccessStreamAsync(canvasDevice, capturedFrame);
    }

    private static GraphicsCaptureItem CreateCaptureItemForWindow(IntPtr hwnd)
    {
        // Windows.Graphics.Capture.GraphicsCaptureItem? item = null;
        // Windows.Graphics.Capture.IGraphicsCaptureItemInterop
        return CaptureHelper.CreateItemForWindow(new Windows.Win32.Foundation.HWND(hwnd))!;
    }

    private static async Task<IRandomAccessStream> SaveFrameToRandomAccessStreamAsync(
        CanvasDevice canvasDevice,
        Direct3D11CaptureFrame capturedFrame)
    {
        // Create a Win2D CanvasBitmap from the Direct3D11 capture frame
        using var canvasBitmap = CanvasBitmap.CreateFromDirect3D11Surface(
            canvasDevice,
            capturedFrame.Surface);

        // Create an in-memory random access stream
        var randomAccessStream = new InMemoryRandomAccessStream();

        // Save the CanvasBitmap as a PNG to the stream
        await canvasBitmap.SaveAsync(randomAccessStream, CanvasBitmapFileFormat.Png);
        await randomAccessStream.FlushAsync();

        // Return the stream
        return randomAccessStream;
    }
}
