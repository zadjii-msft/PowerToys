// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Graphics.Capture;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.System.WinRT.Graphics.Capture;
using WinRT.Interop;

namespace Microsoft.CmdPal.Ext.WindowWalker;

// public static class CaptureHelper
// {
//    [DllImport("Windows.Graphics.Capture.dll", EntryPoint = "CreateCaptureItemForWindow", SetLastError = true)]
//    private static extern nint CreateCaptureItemForWindow(nint hWnd);

// public static GraphicsCaptureItem? CreateItemForWindow(nint hwnd)
//    {
//        // Ensure GraphicsCaptureItem is supported
//        if (!GraphicsCaptureSession.IsSupported())
//        {
//            throw new NotSupportedException("Graphics Capture API is not supported on this system.");
//        }

// // Get the IGraphicsCaptureItem interface for the window
//        var factory = GetActivationFactory<IGraphicsCaptureItemInterop>();
//        if (factory == null)
//        {
//            throw new InvalidOperationException("Failed to get IGraphicsCaptureItemInterop factory.");
//        }

// // Create the capture item for the given window
//        factory.CreateForWindow(hwnd, typeof(GraphicsCaptureItem).GUID, out var captureItemPointer);

// // Marshal the pointer to a GraphicsCaptureItem
//        return Marshal.GetObjectForIUnknown(captureItemPointer) as GraphicsCaptureItem;
//    }

// private static T? GetActivationFactory<T>()
//        where T : class
//    {
//        var factoryType = typeof(T).GUID;
//        Marshal.ThrowExceptionForHR(RoGetActivationFactory(
//            Marshal.StringToHGlobalUni("Windows.Graphics.Capture.GraphicsCaptureItem"),
//            ref factoryType,
//            out var factoryPointer));

// return Marshal.GetObjectForIUnknown(factoryPointer) as T;
//    }

// [DllImport("combase.dll", SetLastError = true, PreserveSig = false)]
//    private static extern int RoGetActivationFactory(nint classId, ref Guid interfaceId, out nint factory);
// }

// [ComImport]
// [Guid("79c3f95b-31f7-4ec2-a464-632ef5d30760")]
// [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
// internal interface IGraphicsCaptureItemInterop
// {
//    void CreateForWindow(nint window, [In] ref Guid iid, out nint result);
// }
public static class CaptureHelper
{
    private static readonly Guid GraphicsCaptureItemGuid = new("79C3F95B-31F7-4EC2-A464-632EF5D30760");

    internal static void SetWindow(this GraphicsCapturePicker picker, HWND hwnd)
    {
        InitializeWithWindow.Initialize(picker, hwnd);
    }

    internal static GraphicsCaptureItem CreateItemForWindow(HWND hwnd)
    {
        GraphicsCaptureItem? item = null;
        unsafe
        {
            item = CreateItemForCallback((IGraphicsCaptureItemInterop interop, Guid* guid) =>
            {
                interop.CreateForWindow(hwnd, guid, out var raw);
                return raw;
            });
        }

        return item;
    }

    internal static GraphicsCaptureItem CreateItemForMonitor(HMONITOR hmon)
    {
        GraphicsCaptureItem? item = null;
        unsafe
        {
            item = CreateItemForCallback((IGraphicsCaptureItemInterop interop, Guid* guid) =>
            {
                interop.CreateForMonitor(hmon, guid, out var raw);
                return raw;
            });
        }

        return item;
    }

    private unsafe delegate object InteropCallback(IGraphicsCaptureItemInterop interop, Guid* guid);

    private static GraphicsCaptureItem CreateItemForCallback(InteropCallback callback)
    {
        var interop = GraphicsCaptureItem.As<IGraphicsCaptureItemInterop>();
        GraphicsCaptureItem? item = null;
        unsafe
        {
            var guid = GraphicsCaptureItemGuid;
            var guidPointer = (Guid*)Unsafe.AsPointer(ref guid);
            var raw = Marshal.GetIUnknownForObject(callback(interop, guidPointer));
            item = GraphicsCaptureItem.FromAbi(raw);
            Marshal.Release(raw);
        }

        return item;
    }
}
