// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Ext.WebSearch.Helpers;
using Microsoft.CmdPal.Ext.WebSearch.Pages;
using Microsoft.CmdPal.Ext.WebSearch.Properties;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace Microsoft.CmdPal.Ext.WebSearch;

public partial class WebSearchCommandsProvider : CommandProvider
{
    private readonly WebSearchTopLevelCommandItem _webSearchCommand;
    private readonly SettingsManager _settingsManager = new();

    public WebSearchCommandsProvider()
    {
        DisplayName = Resources.extension_name;

        _webSearchCommand = new(_settingsManager)
        {
            MoreCommands = [new CommandContextItem(new SettingsPage(_settingsManager))],
        };
    }

    public override ICommandItem[] TopLevelCommands() => [_webSearchCommand];
}
