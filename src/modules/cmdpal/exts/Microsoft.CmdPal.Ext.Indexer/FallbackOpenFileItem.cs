// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using Microsoft.CmdPal.Ext.Indexer.Data;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace Microsoft.CmdPal.Ext.Indexer;

internal sealed partial class FallbackOpenFileItem : FallbackCommandItem
{
    public FallbackOpenFileItem()
        : base(new NoOpCommand())
    {
        Title = string.Empty;
        Subtitle = string.Empty;
    }

    public override void UpdateQuery(string query)
    {
        if (Path.Exists(query))
        {
            var item = new IndexerItem() { FullPath = query, FileName = Path.GetFileName(query) };
            var listItemForUs = new IndexerListItem(item, IncludeBrowseCommand.AsDefault);
            Command = listItemForUs.Command;
            MoreCommands = listItemForUs.MoreCommands;
            Subtitle = item.FullPath;
            Title = item.FileName;
            Icon = listItemForUs.Icon;
        }
        else
        {
            Title = string.Empty;
            Subtitle = string.Empty;
        }
    }
}
