// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using Microsoft.CmdPal.Extensions.Helpers;

namespace SchedulerExtension;

internal sealed partial class ToDoAction : InvokableCommand
{
    private readonly CmdPalToDo _toDo;

    internal ToDoAction(CmdPalToDo toDo)
    {
        this._toDo = toDo;
        this.Name = "Mark completed";
        this.Icon = new("\uE8A7");
    }

    public override CommandResult Invoke()
    {
        // change this function to open preview of task, changed a bit already to resolve build errors
        Process.Start(new ProcessStartInfo(_toDo.Status.ToString()) { UseShellExecute = true });
        return CommandResult.KeepOpen();
    }
}
