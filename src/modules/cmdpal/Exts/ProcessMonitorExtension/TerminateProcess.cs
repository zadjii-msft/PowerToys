﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using Microsoft.CmdPal.Extensions.Helpers;

namespace ProcessMonitorExtension;

internal sealed partial class TerminateProcess : InvokableCommand
{
    private readonly ProcessItem _process;

    public TerminateProcess(ProcessItem process)
    {
        _process = process;
        Icon = new("\ue74d");
        Name = "End task";
    }

    public override CommandResult Invoke()
    {
        var process = Process.GetProcessById(_process.ProcessId);
        process.Kill();
        return CommandResult.KeepOpen();
    }
}
