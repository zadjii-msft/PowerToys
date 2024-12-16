// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using MSDASC;

namespace Microsoft.CmdPal.Ext.Indexer.Native;

internal sealed class NativeHelpers
{
    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    public static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct SHELLEXECUTEINFO
    {
        public int CbSize;
        public uint FMask;
        public IntPtr Hwnd;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string LpVerb;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string LpFile;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string LpParameters;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string LpDirectory;
        public int NShow;
        public IntPtr HInstApp;
        public IntPtr LpIDList;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string LpClass;
        public IntPtr HkeyClass;
        public uint DwHotKey;
        public IntPtr HIcon;
        public IntPtr HProcess;
    }

    public const int SWSHOWNORMAL = 1;
    public const int SWSHOW = 5;
    public const uint SEEMASKINVOKEIDLIST = 12;

    [DllImport("ole32.dll", CharSet = CharSet.Unicode)]
    public static extern int CoCreateInstance(
        [In] Guid rclsid,
        [MarshalAs(UnmanagedType.IUnknown)] object pUnkOuter,
        uint dwClsContext,
        [In] Guid riid,
        [MarshalAs(UnmanagedType.IUnknown)] out object ppv);

    [DllImport("oleaut32.dll")]
    public static extern int GetErrorInfo(uint dwReserved, out IErrorInfo ppErrorInfo);
}
