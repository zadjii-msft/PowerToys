// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.CmdPal.Ext.WindowsTerminal.Commands;
using Microsoft.CmdPal.Ext.WindowsTerminal.Helpers;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.Email.DataProvider;
using static System.Formats.Asn1.AsnWriter;

namespace Microsoft.CmdPal.Ext.WindowsTerminal;

internal sealed partial class TestSettingsListPage : ListPage
{
    private readonly List<ToggleSetting> _settings = new();

    public TestSettingsListPage()
    {
        Icon = new(string.Empty);
        Name = "Testing Settings";
        _settings.Add(WindowsTerminalCommandsProvider.OpenNewTab);
        _settings.Add(WindowsTerminalCommandsProvider.ShowHiddenProfiles);
        _settings.Add(WindowsTerminalCommandsProvider.OpenQuake);
    }

    private ListItem[] GetSettings()
    {
        var result = new List<ListItem>();
        foreach (var setting in _settings)
        {
            result.Add(new ListItem(new NoOpCommand())
            {
                Title = setting.Label,
                Subtitle = setting.Value,
            });
        }

        return result.ToArray();
    }

    public override ISection[] GetItems()
    {
        return [
            new ListSection()
            {
                Title = "Profiles",
                Items = GetSettings(),
            }
            ];
    }
}
