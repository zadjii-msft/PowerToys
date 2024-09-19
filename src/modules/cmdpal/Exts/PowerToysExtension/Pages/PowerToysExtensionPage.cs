// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Microsoft.UI.Xaml.Controls;
using PowerToysExtension.Actions;

namespace PowerToysExtension;

internal sealed partial class PowerToysExtensionPage : ListPage
{
    public PowerToysExtensionPage()
    {
        Icon = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory.ToString(), "Assets\\PowerToys.ico"));
        Name = "PowerToys Utilities";
    }

    public override ISection[] GetItems()
    {
        return [
            new ListSection()
            {
                Items = [
                    new ListItem(new NoOpCommand()) { Title = "TODO: Implement your extension here" },
                    new ListItem(new ColorPickerAction()) { Title = "Color Picker" },
                ],
            }
        ];
    }
}
