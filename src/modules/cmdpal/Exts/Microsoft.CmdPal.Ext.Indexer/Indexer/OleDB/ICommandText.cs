// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.CmdPal.Ext.Indexer.Indexer.OleDB;

[ComImport]
[Guid("0c733a27-2a1c-11ce-ade5-00aa0044773d")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface ICommandText
{
    [PreserveSig]
    int Cancel();

    [PreserveSig]
    int Execute(
        [MarshalAs(UnmanagedType.Interface)] object pUnkOuter,
        [In] ref Guid riid,
        IntPtr pParams,
        out IntPtr pcRowsAffected,
        [Out, MarshalAs(UnmanagedType.Interface)] out object ppRowset);

    [PreserveSig]
    int GetDBSession(
        [In] ref Guid riid,
        [Out, MarshalAs(UnmanagedType.Interface)] out object ppSession);

    [PreserveSig]
    int GetCommandText(
        [Out] out Guid pguidDialect,
        [Out, MarshalAs(UnmanagedType.LPWStr)] out string ppwszCommand);

    [PreserveSig]
    int SetCommandText(
        [In] ref Guid rguidDialect,
        [In, MarshalAs(UnmanagedType.LPWStr)] string pwszCommand);
}
