// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace Microsoft.CmdPal.Ext.WindowsTerminal.Pages;

internal sealed partial class SettingsPage : FormPage
{
    private readonly Settings _settings;

    public override IForm[] Forms()
    {
        var s = _settings.ToForms();
        return s;
    }

    public SettingsPage(Settings settings)
    {
        Name = "Sample Terminal Settings";
        Icon = new(string.Empty);
        _settings = settings;

        _settings.SettingsChanged += SettingsChanged;
    }

    private void SettingsChanged(object sender, Settings args)
    {
        /* Do something with the new settings here */
        var onOff = _settings.GetSetting<bool>("onOff");
        ExtensionHost.LogMessage(new LogMessage() { Message = $"SampleSettingsPage: Changed the value of onOff to {onOff}" });
    }
}
