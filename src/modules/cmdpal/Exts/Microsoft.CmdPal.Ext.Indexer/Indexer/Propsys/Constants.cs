// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Windows.Win32.UI.Shell.PropertiesSystem;

namespace Microsoft.CmdPal.Ext.Indexer.Indexer.Propsys;

internal static class Constants
{
    public static readonly PROPERTYKEY PKEYItemNameDisplay = new() { fmtid = new System.Guid("B725F130-47EF-101A-A5F1-02608C9EEBAC"), pid = 10 };
    public static readonly PROPERTYKEY PKEYItemUrl = new() { fmtid = new System.Guid("49691C90-7E17-101A-A91C-08002B2ECDA9"), pid = 9 };
    public static readonly PROPERTYKEY PKEYKindText = new() { fmtid = new System.Guid("F04BEF95-C585-4197-A2B7-DF46FDC9EE6D"), pid = 100 };
}
