﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.CommandPalette.Extensions.Toolkit;

public partial class CommandContextItem : CommandItem, ICommandContextItem
{
    public bool IsCritical { get; set; }

    public KeyChord RequestedShortcut { get; set; }

    public CommandContextItem(ICommand command)
        : base(command)
    {
    }

    public CommandContextItem(
        Action action,
        string title,
        string subtitle = "",
        string name = "",
        ICommandResult? result = null)
    {
        var c = new AnonymousCommand(action);
        if (!string.IsNullOrEmpty(name))
        {
            c.Name = name;
        }

        if (result != null)
        {
            c.Result = result;
        }

        Command = c;

        Title = title;
        Subtitle = subtitle;
    }
}
