// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.CmdPal.Ext.Indexer.Indexer.OleDB;

[StructLayout(LayoutKind.Explicit)]
public struct DBIDUNION
{
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
    [FieldOffset(0)]
    public IntPtr pwszName; // For string names
    [FieldOffset(0)]
    public uint ulPropid;   // For property IDs
#pragma warning restore SA1307 // Accessible fields should begin with upper-case letter
}
