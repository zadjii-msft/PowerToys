﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.CmdPal.Ext.ClipboardHistory.Pages;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace Microsoft.CmdPal.Ext.ClipboardHistory;

public partial class ClipboardHistoryCommandsProvider : CommandProvider
{
    private readonly ListItem _clipboardHistoryListItem;

    public ClipboardHistoryCommandsProvider()
    {
        _clipboardHistoryListItem = new ListItem(new ClipboardHistoryListPage())
        {
            Title = "Search Clipboard History",
            Icon = new IconInfo("ClipboardHistory"),
        };

        DisplayName = $"Settings";
        Icon = new("ClipboardHistory");
    }

    public override IListItem[] TopLevelCommands()
    {
        return [_clipboardHistoryListItem];
    }
}
