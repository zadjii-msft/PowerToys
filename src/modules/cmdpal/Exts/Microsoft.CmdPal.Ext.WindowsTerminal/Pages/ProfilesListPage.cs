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
using static System.Formats.Asn1.AsnWriter;

namespace Microsoft.CmdPal.Ext.WindowsTerminal.Pages;

internal sealed partial class ProfilesListPage : ListPage
{
    private readonly TerminalQuery _terminalQuery = new();
    private readonly Settings _terminalSettings;
    private readonly Dictionary<string, BitmapImage> _logoCache = new();

    private bool showHiddenProfiles;
    private bool openNewTab;
    private bool openQuake;

    public ProfilesListPage(Settings terminalSettings)
    {
        Icon = new(string.Empty);
        Name = "Windows Terminal Profiles";
        showHiddenProfiles = terminalSettings.GetSetting<bool>(nameof(WindowsTerminalCommandsProvider.ShowHiddenProfiles));
        openNewTab = terminalSettings.GetSetting<bool>(nameof(WindowsTerminalCommandsProvider.OpenNewTab));
        openQuake = terminalSettings.GetSetting<bool>(nameof(WindowsTerminalCommandsProvider.OpenQuake));
        _terminalSettings = terminalSettings;
    }

#pragma warning disable SA1108
    public List<ListItem> Query()
    {
        showHiddenProfiles = _terminalSettings.GetSetting<bool>(nameof(WindowsTerminalCommandsProvider.ShowHiddenProfiles));
        openNewTab = _terminalSettings.GetSetting<bool>(nameof(WindowsTerminalCommandsProvider.OpenNewTab));
        openQuake = _terminalSettings.GetSetting<bool>(nameof(WindowsTerminalCommandsProvider.OpenQuake));

        var profiles = _terminalQuery.GetProfiles();

        var result = new List<ListItem>();

        foreach (var profile in profiles)
        {
            if (profile.Hidden && !showHiddenProfiles) // TODO: hmmm, probably need settings to do this --> && !_showHiddenProfiles)
            {
                continue;
            }

            result.Add(new ListItem(new LaunchProfileCommand(profile.Terminal.AppUserModelId, profile.Name, profile.Terminal.LogoPath, openNewTab, openQuake))
            {
                Title = profile.Name,
                Subtitle = profile.Terminal.DisplayName,
                MoreCommands = [
                    new CommandContextItem(new LaunchProfileAsAdminCommand(profile.Terminal.AppUserModelId, profile.Name, openNewTab, openQuake)),
                ],

                // Icon = () => GetLogo(profile.Terminal),
                // Action = _ =>
                // {
                //    Launch(profile.Terminal.AppUserModelId, profile.Name);
                //    return true;
                // },
                // ContextData = profile,
#pragma warning restore SA1108
            });
        }

        return result;
    }

    public override IListItem[] GetItems()
    {
        return Query().ToArray();
    }

    private BitmapImage GetLogo(TerminalPackage terminal)
    {
        var aumid = terminal.AppUserModelId;

        if (!_logoCache.TryGetValue(aumid, out BitmapImage value))
        {
            value = terminal.GetLogo();
            _logoCache.Add(aumid, value);
        }

        return value;
    }
}
