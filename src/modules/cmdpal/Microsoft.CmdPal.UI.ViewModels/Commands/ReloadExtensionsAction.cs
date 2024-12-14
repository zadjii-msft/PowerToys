// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.Messaging;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Microsoft.CmdPal.UI.ViewModels.Messages;

namespace Microsoft.CmdPal.UI.ViewModels.BuiltinCommands;

public partial class ReloadExtensionsAction : InvokableCommand, IFallbackHandler
{
    public ReloadExtensionsAction()
    {
        Icon = new("\uE72C"); // Refresh icon
    }

    public override ICommandResult Invoke()
    {
        WeakReferenceMessenger.Default.Send<ReloadCommandsMessage>();
        return CommandResult.GoHome();
    }

    public void UpdateQuery(string query) => Name = query.StartsWith('r') ? "Reload" : string.Empty;
}
