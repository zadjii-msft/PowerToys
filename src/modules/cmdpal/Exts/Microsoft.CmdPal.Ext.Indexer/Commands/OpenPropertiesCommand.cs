// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using Microsoft.CmdPal.Ext.Indexer.Data;
using Microsoft.CmdPal.Ext.Indexer.Native;
using Microsoft.CmdPal.Ext.Indexer.Utils;
using Microsoft.CmdPal.Extensions.Helpers;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Microsoft.CmdPal.Ext.Indexer.Commands;

internal sealed partial class OpenPropertiesCommand : InvokableCommand
{
    private readonly IndexerItem _item;

    private static unsafe bool ShowFileProperties(string filename)
    {
        var propertiesPtr = Marshal.StringToHGlobalUni("properties");
        var filenamePtr = Marshal.StringToHGlobalUni(filename);

        try
        {
            var filenamePCWSTR = new PCWSTR((char*)filenamePtr);
            var propertiesPCWSTR = new PCWSTR((char*)propertiesPtr);

            var info = new SHELLEXECUTEINFOW
            {
                cbSize = (uint)Marshal.SizeOf<SHELLEXECUTEINFOW>(),
                lpVerb = propertiesPCWSTR,
                lpFile = filenamePCWSTR,
                nShow = (int)SHOW_WINDOW_CMD.SW_SHOW,
                fMask = NativeHelpers.SEEMASKINVOKEIDLIST,
            };

            return PInvoke.ShellExecuteEx(ref info);
        }
        finally
        {
            Marshal.FreeHGlobal(filenamePtr);
            Marshal.FreeHGlobal(propertiesPtr);
        }
    }

    internal OpenPropertiesCommand(IndexerItem item)
    {
        this._item = item;
        this.Name = "Properties";
        this.Icon = new("\uE90F");
    }

    public override CommandResult Invoke()
    {
        try
        {
            ShowFileProperties(_item.FullPath);
        }
        catch (Exception ex)
        {
            Logger.LogError("Error showing file properties: ", ex);
        }

        return CommandResult.GoHome();
    }
}
