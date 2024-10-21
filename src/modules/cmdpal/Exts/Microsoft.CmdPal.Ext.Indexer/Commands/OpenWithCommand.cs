// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.CmdPal.Ext.Indexer.Data;
using Microsoft.CmdPal.Ext.Indexer.Native;
using Microsoft.CmdPal.Extensions.Helpers;

namespace Microsoft.CmdPal.Ext.Indexer.Commands;

internal sealed partial class OpenWithCommand : InvokableCommand
{
    private readonly IndexerItem _item;

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr ShellExecute(
        IntPtr hwnd,
        [MarshalAs(UnmanagedType.LPWStr)] string lpOperation,
        [MarshalAs(UnmanagedType.LPWStr)] string lpFile,
        [MarshalAs(UnmanagedType.LPWStr)] string lpParameters,
        [MarshalAs(UnmanagedType.LPWStr)] string lpDirectory,
        int nShowCmd);

    private static bool OpenWith(string filename)
    {
        NativeHelpers.SHELLEXECUTEINFO info = new NativeHelpers.SHELLEXECUTEINFO { };
        info.CbSize = Marshal.SizeOf(info);
        info.LpVerb = "openas";
        info.LpFile = filename;
        info.NShow = NativeHelpers.SWSHOWNORMAL;
        return NativeHelpers.ShellExecuteEx(ref info);
    }

    internal OpenWithCommand(IndexerItem item)
    {
        this._item = item;
        this.Name = "Open with";
        this.Icon = new("\uE7AC");
    }

    public override CommandResult Invoke()
    {
        using (var process = new Process())
        {
            process.StartInfo.FileName = _item.FullPath;
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.Verb = "openas";

            try
            {
                process.Start();
            }
            catch (Win32Exception /*ex*/)
            {
                // Log.Exception($"Unable to open {path}: {ex.Message}", ex, MethodBase.GetCurrentMethod().DeclaringType);
            }
        }

        return CommandResult.GoHome();
    }
}
