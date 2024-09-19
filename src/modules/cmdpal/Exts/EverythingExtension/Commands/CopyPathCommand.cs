// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using Microsoft.CmdPal.Extensions.Helpers;

internal sealed partial class CopyPathCommand : InvokableCommand
{
    private readonly string _fullname;

    internal CopyPathCommand(string fullname)
    {
        _fullname = fullname;
        Name = "Copy path";
        Icon = new("\ue8c8");
    }

    public override CommandResult Invoke()
    {
        // var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
        // dataPackage.SetText(_fullname);
        // Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
        return CommandResult.KeepOpen();
    }
}
