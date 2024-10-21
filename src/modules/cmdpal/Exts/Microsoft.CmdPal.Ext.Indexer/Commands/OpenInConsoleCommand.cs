// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Microsoft.CmdPal.Ext.Indexer.Data;
using Microsoft.CmdPal.Extensions.Helpers;

namespace Microsoft.CmdPal.Ext.Indexer.Commands;

internal sealed partial class OpenInConsoleCommand : InvokableCommand
{
    private readonly IndexerItem _item;

    internal OpenInConsoleCommand(IndexerItem item)
    {
        this._item = item;
        this.Name = "Open path in console";
        this.Icon = new("\uE756");
    }

    public override CommandResult Invoke()
    {
        using (var process = new Process())
        {
            process.StartInfo.WorkingDirectory = Path.GetDirectoryName(_item.FullPath);
            process.StartInfo.FileName = "cmd.exe";

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
