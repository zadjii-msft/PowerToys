// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace Microsoft.CmdPal.Ext.Apps.Programs;

// NOTE this is pretty close to what we'd put in the SDK
internal sealed partial class OpenPathCommand(string target) : InvokableCommand
{
    private readonly string _target = target;

    internal static async Task LaunchTarget(string t)
    {
        await Task.Run(() =>
        {
            Process.Start(new ProcessStartInfo(t) { UseShellExecute = true });
        });
    }

    public override CommandResult Invoke()
    {
        _ = LaunchTarget(_target).ConfigureAwait(false);

        return CommandResult.GoHome();
    }
}
