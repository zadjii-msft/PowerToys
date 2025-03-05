// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CmdPal.Ext.Indexer.Data;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace Microsoft.CmdPal.Ext.Indexer;

public sealed partial class DirectoryPage : ListPage
{
    private readonly List<IListItem> _indexerListItems = [];
    private readonly string _path;

    public DirectoryPage(string path)
    {
        _path = path;
        Icon = Icons.FileExplorerSegoe;
        Name = "Browse"; // TODO:LOC
        Title = path;
    }

    public override IListItem[] GetItems()
    {
        if (!Path.Exists(_path))
        {
            EmptyContent = new CommandItem(title: "This file doesn't exist!"); // TODO:LOC
            return [];
        }

        var attr = File.GetAttributes(_path);

        // detect whether its a directory or file
        if ((attr & FileAttributes.Directory) != FileAttributes.Directory)
        {
            EmptyContent = new CommandItem(title: "This is a file, not a folder!"); // TODO:LOC
            return [];
        }

        var contents = Directory.EnumerateFileSystemEntries(_path);
        return contents
            .Select(s => new IndexerItem() { FullPath = s, FileName = Path.GetFileName(s) })
            .Select(i => new IndexerListItem(i))
            .ToArray();
    }
}
