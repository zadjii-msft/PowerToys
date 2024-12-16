// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace Microsoft.CmdPal.Ext.Indexer;

public partial class IndexerActionsProvider : CommandProvider
{
    public IndexerActionsProvider()
    {
        DisplayName = "Indexer Commands";
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return [
            new CommandItem(new IndexerPage())
            {
                Title = "File search",
                Subtitle = "Search indexed files",
            }
        ];
    }
}
