// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace SchedulerExtension;

internal sealed partial class SchedulerExtensionPage : ListPage
{
    public SchedulerExtensionPage()
    {
        Icon = new(string.Empty);
        Name = "Best scheduler ever.";
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
