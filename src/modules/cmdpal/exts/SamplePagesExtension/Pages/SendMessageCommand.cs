// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Windows.Foundation;

namespace SamplePagesExtension;

internal sealed partial class SendMessageCommand : InvokableCommand
{
    private static int sentMessages;

    public override ICommandResult Invoke()
    {
        var kind = MessageState.Info;
        switch (sentMessages % 4)
        {
            case 0: kind = MessageState.Info; break;
            case 1: kind = MessageState.Success; break;
            case 2: kind = MessageState.Warning; break;
            case 3: kind = MessageState.Error; break;
        }

        var message = new StatusMessage() { Message = $"I am status message no.{sentMessages++}", State = kind };
        ExtensionHost.ShowStatus(message);
        return CommandResult.KeepOpen();
    }
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Sample code sometimes makes more sense in a single file")]
internal sealed partial class SendSingleMessageItem : ListItem
{
    private readonly SingleMessageCommand _command;

    public SendSingleMessageItem()
        : base(new SingleMessageCommand())
    {
        Title = "I send a single message";
        _command = (SingleMessageCommand)Command;
        _command.UpdateListItem += (sender, args) =>
        {
            Title = _command.Shown ? "Hide message" : "I send a single message";
        };
    }
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Sample code sometimes makes more sense in a single file")]
internal sealed partial class SingleMessageCommand : InvokableCommand
{
    public event TypedEventHandler<SingleMessageCommand, object> UpdateListItem;

    private readonly StatusMessage _myMessage = new() { Message = "I am a status message" };

    public bool Shown { get; private set; }

    public override ICommandResult Invoke()
    {
        if (Shown)
        {
            ExtensionHost.HideStatus(_myMessage);
        }
        else
        {
            ExtensionHost.ShowStatus(_myMessage);
        }

        Shown = !Shown;
        Name = Shown ? "Hide" : "Show";
        UpdateListItem?.Invoke(this, null);
        return CommandResult.KeepOpen();
    }
}
