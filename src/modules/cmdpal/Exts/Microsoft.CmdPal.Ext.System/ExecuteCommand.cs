// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CmdPal.Ext.System.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;
using static System.Net.Mime.MediaTypeNames;

namespace Microsoft.CmdPal.Ext.System;

public sealed partial class ExecuteCommand : InvokableCommand
{
    public ExecuteCommand(bool confirm, string confirmationMessage, Action command)
    {
        _command = command;
        _confirm = confirm;
        _confirmationMessage = confirmationMessage;
    }

    public override CommandResult Invoke()
    {
        ResultHelper.ExecuteCommand(_confirm, _confirmationMessage, _command);
        return CommandResult.Dismiss();
    }

    private bool _confirm;
    private string _confirmationMessage;
    private Action _command;
}
