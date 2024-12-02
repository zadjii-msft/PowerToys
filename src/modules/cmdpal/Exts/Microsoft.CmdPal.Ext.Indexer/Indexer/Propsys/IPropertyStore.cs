// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using Microsoft.CmdPal.Ext.Indexer.Utils;

namespace Microsoft.CmdPal.Ext.Indexer.Indexer.Propsys;

[ComImport]
[Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IPropertyStore
{
    [PreserveSig]
    int GetCount(out uint cProps);

    [PreserveSig]
    int GetAt(uint iProp, out PROPERTYKEY pkey);

    [PreserveSig]
    int GetValue([In] ref PROPERTYKEY key, [Out] out PROPVARIANT pv);

    [PreserveSig]
    int SetValue([In] ref PROPERTYKEY key, [In] ref PROPVARIANT propvar);

    [PreserveSig]
    int Commit();
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct PROPERTYKEY
{
#pragma warning disable SA1307 // AccessibleFieldsMustBeginWithUpperCaseLetter
    public Guid fmtid;
    public uint pid;
#pragma warning restore SA1307 // AccessibleFieldsMustBeginWithUpperCaseLetter
}

[StructLayout(LayoutKind.Sequential)]
public struct PROPVARIANT : IDisposable
{
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
    public ushort vt;
    public ushort wReserved1;
    public ushort wReserved2;
    public ushort wReserved3;
    public PROPVARIANTUnion unionValue;
#pragma warning restore SA1307 // Accessible fields should begin with upper-case letter

    // Implement IDisposable to clean up unmanaged resources
    public void Dispose()
    {
        var res = PropVariantClear(ref this);
        if (res != 0)
        {
            Logger.LogError("Error in PropVariantClear: " + res);
        }
    }

    [DllImport("ole32.dll")]
    private static extern int PropVariantClear(ref PROPVARIANT pvar);
}

[StructLayout(LayoutKind.Explicit)]
public struct PROPVARIANTUnion
{
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
    [FieldOffset(0)]
    public IntPtr pszVal; // For VT_LPWSTR

    [FieldOffset(0)]
    public IntPtr pwszVal; // For VT_LPWSTR (Unicode string)

    [FieldOffset(0)]
    public IntPtr punkVal; // For VT_UNKNOWN

    [FieldOffset(0)]
    public sbyte cVal;     // For VT_I1

    [FieldOffset(0)]
    public byte bVal;      // For VT_UI1

    [FieldOffset(0)]
    public short iVal;     // For VT_I2

    [FieldOffset(0)]
    public ushort uiVal;   // For VT_UI2

    [FieldOffset(0)]
    public int lVal;       // For VT_I4

    [FieldOffset(0)]
    public uint ulVal;     // For VT_UI4

    [FieldOffset(0)]
    public long hVal;      // For VT_I8

    [FieldOffset(0)]
    public ulong uhVal;    // For VT_UI8
#pragma warning restore SA1307 // Accessible fields should begin with upper-case letter
}
