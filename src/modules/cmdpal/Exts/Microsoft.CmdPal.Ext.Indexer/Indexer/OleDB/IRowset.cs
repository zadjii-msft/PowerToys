// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.CmdPal.Ext.Indexer.Indexer.OleDB;

[ComImport]
[Guid("0c733a7c-2a1c-11ce-ade5-00aa0044773d")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IRowset
{
    [PreserveSig]
    int AddRefRows(
        uint cRows,
        [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] IntPtr[] rghRows,
        [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] uint[] rgRefCounts,
        [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] int[] rgRowStatus);

    [PreserveSig]
    int GetData(
        IntPtr hRow,
        IntPtr hAccessor,
        IntPtr pData);

    [PreserveSig]
    int GetNextRows(
       IntPtr hReserved,               // HCHAPTER hReserved
       long lRowsOffset,               // DBROWOFFSET lRowsOffset
       long cRows,                     // DBROWCOUNT cRows
       out uint pcRowsObtained,        // DBCOUNTITEM *pcRowsObtained
       out IntPtr prghRows);             // HROW **prghRows

    [PreserveSig]
    int ReleaseRows(
        uint cRows,                                // DBCOUNTITEM cRows
        [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] IntPtr[] rghRows, // HROW rghRows[]
        IntPtr rgRowOptions,                       // DBROWOPTIONS rgRowOptions[] (can be null)
        [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] uint[] rgRefCounts, // ULONG rgRefCounts[] (can be null)
        [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] int[] rgRowStatus);
}
