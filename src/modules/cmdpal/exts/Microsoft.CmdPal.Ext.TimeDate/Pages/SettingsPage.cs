﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Ext.TimeDate.Helpers;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace Microsoft.CmdPal.Ext.TimeDate.Pages;

internal sealed partial class SettingsPage : FormPage
{
    private readonly Microsoft.CmdPal.Extensions.Helpers.Settings _settings;
    private readonly SettingsManager _settingsManager;

    public override IForm[] Forms()
    {
        var s = _settings.ToForms();
        return s;
    }

    public SettingsPage()
    {
        Name = Properties.Resources.settings_page_name;
        Icon = new("\uE713"); // Settings icon
        _settingsManager = SettingsManager.Instance;
        _settings = _settingsManager.GetSettings();

        _settings.SettingsChanged += SettingsChanged;
    }

    private void SettingsChanged(object sender, Microsoft.CmdPal.Extensions.Helpers.Settings args) => _settingsManager.SaveSettings();
}
