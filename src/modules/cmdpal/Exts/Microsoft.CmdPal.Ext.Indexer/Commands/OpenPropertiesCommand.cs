// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using Microsoft.CmdPal.Ext.Indexer.Data;
using Microsoft.CmdPal.Ext.Indexer.Native;
using Microsoft.CmdPal.Extensions.Helpers;

namespace Microsoft.CmdPal.Ext.Indexer.Commands;

internal sealed partial class OpenPropertiesCommand : InvokableCommand
{
    private readonly IndexerItem _item;

    private static bool ShowFileProperties(string filename)
    {
        NativeHelpers.SHELLEXECUTEINFO info = new NativeHelpers.SHELLEXECUTEINFO { };
        info.CbSize = Marshal.SizeOf(info);
        info.LpVerb = "properties";
        info.LpFile = filename;
        info.NShow = NativeHelpers.SWSHOW;
        info.FMask = NativeHelpers.SEEMASKINVOKEIDLIST;
        return NativeHelpers.ShellExecuteEx(ref info);
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
            Console.WriteLine("Error showing file properties: " + ex.Message);
        }

        return CommandResult.GoHome();
    }
}
