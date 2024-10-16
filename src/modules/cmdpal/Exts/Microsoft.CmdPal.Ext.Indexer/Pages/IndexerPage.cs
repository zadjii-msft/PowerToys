// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace Microsoft.CmdPal.Ext.Indexer;

internal sealed partial class IndexerPage : ListPage
{
    public IndexerPage()
    {
        Icon = new("\ue729");
        Name = "Indexer";
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
