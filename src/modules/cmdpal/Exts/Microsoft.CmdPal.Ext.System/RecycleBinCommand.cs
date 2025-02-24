// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Ext.System.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace Microsoft.CmdPal.Ext.System;

public sealed partial class RecycleBinCommand : InvokableCommand
{
    public RecycleBinCommand(bool settingEmptyRBSuccesMsg)
    {
        _settingEmptyRBSuccesMsg = settingEmptyRBSuccesMsg;
    }

    public override CommandResult Invoke()
    {
        ResultHelper.EmptyRecycleBinAsync(_settingEmptyRBSuccesMsg);

        return CommandResult.Dismiss();
    }

    private bool _settingEmptyRBSuccesMsg;
}
