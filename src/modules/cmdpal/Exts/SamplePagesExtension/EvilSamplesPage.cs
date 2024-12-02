// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace SamplePagesExtension;

public partial class EvilSamplesPage : ListPage
{
    private readonly IListItem[] _commands = [
       new ListItem(new EvilSampleListPage())
       {
           Title = "List Page without items",
           Subtitle = "Throws exception on GetItems",
       },
    ];

    public EvilSamplesPage()
    {
        Name = "Evil Samples";
        Icon = new("👿"); // Info
    }

    public override IListItem[] GetItems() => _commands;
}
