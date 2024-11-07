// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.CmdPal.Ext.Indexer.Indexer.OleDB;

[StructLayout(LayoutKind.Sequential)]
public struct DBID
{
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
    public Guid uGuid;
    public uint eKind;  // Should be an enum, but uint works
    public DBIDUNION uName;
#pragma warning restore SA1307 // Accessible fields should begin with upper-case letter
}
