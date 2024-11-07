// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.CmdPal.Ext.Indexer.Indexer.OleDB;

[ComImport]
[Guid("0c733aaf-2a1c-11ce-ade5-00aa0044773d")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IGetRow
{
    [PreserveSig]
    int GetRowFromHROW(
        [MarshalAs(UnmanagedType.Interface)] object pUnkOuter,
        [In] IntPtr hRow,
        [In] ref Guid riid,
        [Out, MarshalAs(UnmanagedType.Interface)] out object ppUnk);
}
