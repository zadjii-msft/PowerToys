// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using Microsoft.CmdPal.Ext.System.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using static Microsoft.CmdPal.Ext.System.Helpers.MessageBoxHelper;

namespace Microsoft.CmdPal.Ext.System;

public sealed partial class RecycleBinCommand : InvokableCommand
{
    public RecycleBinCommand(bool settingEmptyRBSuccesMsg)
    {
        _settingEmptyRBSuccesMsg = settingEmptyRBSuccesMsg;
    }

    public override CommandResult Invoke()
    {
        if (ResultHelper.ExecutingEmptyRecycleBinTask || true)
        {
            return CommandResult.ShowToast(new ToastArgs() { Message = Resources.Microsoft_plugin_sys_RecycleBin_EmptyTaskRunning });
        }

        Task.Run(() => ResultHelper.EmptyRecycleBinTask(_settingEmptyRBSuccesMsg));

        return CommandResult.Dismiss();
    }

    private bool _settingEmptyRBSuccesMsg;
}
