// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace Microsoft.CmdPal.Ext.WindowsTerminal;

internal sealed partial class TestPage : FormPage
{
    private readonly ToggleSetting _openNewTab;
    private readonly ToggleSetting _showHiddenProfiles;
    private readonly ToggleSetting _openQuake;
    private readonly SettingsPage _settingsForm;

    public override IForm[] Forms() => [_settingsForm];

    public TestPage(ToggleSetting openNewTab, ToggleSetting showHiddenProfiles, ToggleSetting openQuake)
    {
        Name = "Windows Terminal Settings";
        Icon = new("https://imgflip.com/s/meme/Mocking-Spongebob.jpg");
        _openNewTab = openNewTab;
        _showHiddenProfiles = showHiddenProfiles;
        _openQuake = openQuake;
        _settingsForm = new SettingsPage(new() { _openNewTab, _showHiddenProfiles, _openQuake }, "C:\\temp\\test.json");
    }
}
