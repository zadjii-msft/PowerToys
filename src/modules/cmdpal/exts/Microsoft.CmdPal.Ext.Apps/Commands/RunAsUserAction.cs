// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.CmdPal.Ext.Apps.Utils;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace Microsoft.CmdPal.Ext.Apps.Commands;

// NOTE this is pretty close to what we'd put in the SDK
internal sealed partial class RunAsUserCommand : InvokableCommand
{
    private readonly string _target;
    private readonly string _parentDir;

    public RunAsUserCommand(string target, string parentDir)
    {
        Name = "Run as different user";
        Icon = new IconInfo("\uE7EE");

        _target = target;
        _parentDir = parentDir;
    }

    internal static async Task RunAsAdmin(string target, string parentDir)
    {
        await Task.Run(() =>
        {
            var info = ShellCommand.GetProcessStartInfo(target, parentDir, string.Empty, ShellCommand.RunAsType.OtherUser);

            Process.Start(info);
        });
    }

    public override CommandResult Invoke()
    {
        _ = RunAsAdmin(_target, _parentDir).ConfigureAwait(false);

        return CommandResult.Dismiss();
    }
}
