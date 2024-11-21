// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CmdPal.Ext.Shell.Helpers;
using Microsoft.CmdPal.Ext.Shell.Properties;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Windows.UI.Input.Spatial;

namespace Microsoft.CmdPal.Ext.Shell.Pages;

internal sealed partial class ShellListPage : DynamicListPage
{
    private readonly ShellListPageHelpers _helper;

    public ShellListPage(SettingsManager settingsManager)
    {
        Icon = new(string.Empty);
        Name = Resources.wox_plugin_cmd_plugin_name;
        _helper = new(settingsManager);
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        RaiseItemsChanged(0);
    }

    public override IListItem[] GetItems()
    {
        return _helper.Query(SearchText).ToArray();
    }
}
