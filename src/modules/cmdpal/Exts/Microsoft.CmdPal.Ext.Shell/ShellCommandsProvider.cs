// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using Microsoft.CmdPal.Ext.Shell.Helpers;
using Microsoft.CmdPal.Ext.Shell.Pages;
using Microsoft.CmdPal.Ext.Shell.Properties;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace Microsoft.CmdPal.Ext.Shell;

public partial class ShellCommandsProvider : CommandProvider
{
    private readonly ListItem _shellPageItem;

    public ShellCommandsProvider()
    {
        DisplayName = Resources.wox_plugin_cmd_plugin_name;
        _shellPageItem = new ListItem(new ShellListPage())
        {
            Title = "testing this out",
        };
    }

    public override IListItem[] TopLevelCommands()
    {
        return [_shellPageItem];
    }
}
