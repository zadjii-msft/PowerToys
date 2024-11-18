﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Ext.WindowsTerminal.Helpers;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace Microsoft.CmdPal.Ext.WindowsTerminal.Pages;

internal sealed partial class SettingsPage : FormPage
{
    private readonly Settings _settings;
    private readonly SettingsManager _settingsManager;

    public override IForm[] Forms()
    {
        var s = _settings.ToForms();
        return s;
    }

    public SettingsPage(SettingsManager settingsManager)
    {
        Name = "Sample Terminal Settings";
        Icon = new(string.Empty);
        _settings = settingsManager.GetSettings();
        _settingsManager = settingsManager;

        _settings.SettingsChanged += SettingsChanged;
    }

    private void SettingsChanged(object sender, Settings args)
    {
        _settingsManager.SaveSettings();
    }
}
