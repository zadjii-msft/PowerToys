// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace MediaControlsExtension;

public partial class MediaCommandsProvider : CommandProvider
{
    public MediaCommandsProvider()
    {
        DisplayName = "Media controls commands";
    }

    private readonly CommandItem[] _commands = [
        new MediaListItem()
    ];

    public override CommandItem[] TopLevelCommands()
    {
        return _commands;
    }
}
