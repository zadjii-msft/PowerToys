// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace TemplateCmdPalExtension;

public partial class TemplateCmdPalExtensionActionsProvider : CommandProvider
{
    public TemplateCmdPalExtensionActionsProvider()
    {
        DisplayName = "TemplateDisplayName Commands";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
    }

    private readonly ICommandItem[] _commands = [
        new CommandItem(new TemplateCmdPalExtensionPage()),
    ];

    public override ICommandItem[] TopLevelCommands() => _commands;
}
