﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Windows.CommandPalette.Extensions;
using Microsoft.Windows.CommandPalette.Extensions.Helpers;
using Windows.Foundation;

namespace DeveloperCommandPalette;

public class QuitAction : InvokableCommand, IFallbackHandler
{
    public event TypedEventHandler<object?, object?>? QuitRequested;

    public QuitAction()
    {
        Icon = new("\uE711");
    }

    public override ICommandResult Invoke()
    {
        QuitRequested?.Invoke(this, new());
        return ActionResult.KeepOpen();
    }

    public void UpdateQuery(string query) {
        if (query.StartsWith('q'))
        {
            this.Name = "Quit";
        }
        else this.Name = "";

    }
}
public class QuitActionProvider : ICommandProvider
{
    public string DisplayName => "";
    public IconDataType Icon => new("");
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
    public void Dispose() => throw new NotImplementedException();
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
    private readonly QuitAction quitAction = new();

    public event TypedEventHandler<object?, object?>? QuitRequested { add => quitAction.QuitRequested += value; remove => quitAction.QuitRequested -= value; }

    public IListItem[] TopLevelCommands()
    {
        return [new ListItem(quitAction) { Subtitle = "Exit Command Palette" }];
    }
}
