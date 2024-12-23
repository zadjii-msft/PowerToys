// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace SamplePagesExtension;

internal sealed partial class SendMessageCommand : InvokableCommand
{
    private static int sentMessages;

    public override ICommandResult Invoke()
    {
        var message = new StatusMessage() { Message = $"I am status message no.{sentMessages++}" };
        ExtensionHost.ShowStatus(message);
        return CommandResult.KeepOpen();
    }
}
