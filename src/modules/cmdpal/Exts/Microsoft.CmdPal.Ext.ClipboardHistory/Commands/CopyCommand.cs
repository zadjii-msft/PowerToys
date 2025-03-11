// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;
using Microsoft.CmdPal.Ext.ClipboardHistory.Models;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace Microsoft.CmdPal.Ext.ClipboardHistory.Commands;

internal sealed partial class CopyCommand : InvokableCommand
{
    private readonly ClipboardItem _clipboardItem;
    private readonly ClipboardFormat _clipboardFormat;

    internal CopyCommand(ClipboardItem clipboardItem, ClipboardFormat clipboardFormat)
    {
        _clipboardItem = clipboardItem;
        _clipboardFormat = clipboardFormat;
        Name = "Copy";
        if (clipboardFormat == ClipboardFormat.Text)
        {
            Icon = new("\xE8C8"); // Copy icon
        }
        else
        {
            Icon = new("\xE8B9"); // Picture icon
        }
    }

    public override CommandResult Invoke()
    {
        var thread = new Thread(() =>
        {
            ClipboardHelper.SetClipboardContent(_clipboardItem, _clipboardFormat);
        });

        // thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        return CommandResult.ShowToast("Copied to clipboard");
    }
}
