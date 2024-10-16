// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CmdPal.Ext.Indexer.Data;
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
        return DoGetItems();
    }

    private static /*async Task<List<IndexerItem>>*/ List<IndexerItem> GetFiles()
    {
        var items = new List<IndexerItem>();

        var currentDir = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        var files = Directory.GetFiles(currentDir);

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);

            items.Add(new IndexerItem()
            {
                FullPath = file,
                FileName = fileName,
            });
        }

        return items;
    }

    private /*async Task<ISection[]>*/ ISection[] DoGetItems()
    {
        List<IndexerItem> items = /*await*/ GetFiles();
        var s = new ListSection()
        {
            Title = "Files", // TODO: localize
            Items = items.Select((item) => new ListItem(new NoOpCommand())
            {
                Title = item.FileName,
                Subtitle = item.FullPath,
            }).ToArray(),
        };

        return [s];
    }
}
