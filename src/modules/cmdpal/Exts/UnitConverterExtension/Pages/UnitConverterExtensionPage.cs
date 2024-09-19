// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace UnitConverterExtension;

internal sealed partial class UnitConverterExtensionPage : ListPage
{
    public UnitConverterExtensionPage()
    {
        Icon = new("https://raw.githubusercontent.com/microsoft/PowerToys/refs/heads/main/src/modules/launcher/Plugins/Community.PowerToys.Run.Plugin.UnitConverter/Images/unitconverter.dark.png");
        Name = "Unit Converter";
    }

    public override ISection[] GetItems()
    {
        return [
            new ListSection()
            {
                Items = [
                    new ListItem(new NoOpCommand()) { Title = "TODO: Implement your extension here" }
                ],
            }
        ];
    }
}
