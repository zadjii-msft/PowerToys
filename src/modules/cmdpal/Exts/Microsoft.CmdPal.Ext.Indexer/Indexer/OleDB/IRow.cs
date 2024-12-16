// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.CmdPal.Ext.Indexer.Indexer.OleDB;

[ComImport]
[Guid("0C733A55-2A1C-11CE-ADE5-00AA0044773D")] // IID_IRow
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IRow
{
    /*[PreserveSig]
    int GetColumns(
        uint cColumns,
        [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] DBCOLUMNACCESS[] rgColumns);*/
}
