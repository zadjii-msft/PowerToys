// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace SamplePagesExtension;

internal sealed partial class SampleSettingsPage : FormPage
{
    private readonly Settings _settings = new();

    public override IForm[] Forms()
    {
        var s = _settings.ToForms();
        return s;
    }

    public SampleSettingsPage()
    {
        Name = "Sample Settings";
        Icon = new(string.Empty);
        _settings.Add(new ToggleSetting("onOff", true)
            {
                Label = "This is a toggle",
                Description = "It produces a simple checkbox",
            });
        _settings.Add(new TextSetting("someText", "initial value")
            {
                Label = "This is a text box",
                Description = "For some string of text",
            });
    }
}
