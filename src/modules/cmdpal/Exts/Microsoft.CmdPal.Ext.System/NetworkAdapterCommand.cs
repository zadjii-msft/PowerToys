// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions.Toolkit;

namespace Microsoft.CmdPal.Ext.System;

public sealed partial class NetworkAdapterCommand : InvokableCommand
{
    public NetworkAdapterCommand()
    {
        text = "test data";
    }

    public override CommandResult Invoke()
    {
        ClipboardHelper.SetText(text);
        return CommandResult.Dismiss();
    }

    private string text;
}
