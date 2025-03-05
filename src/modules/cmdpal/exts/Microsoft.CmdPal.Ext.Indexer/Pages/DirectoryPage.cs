﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CmdPal.Ext.Indexer.Data;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.Storage.Streams;

#nullable enable
namespace Microsoft.CmdPal.Ext.Indexer;

public sealed partial class DirectoryPage : ListPage
{
    private readonly string _path;

    private List<IndexerListItem>? _directoryContents;

    public DirectoryPage(string path)
    {
        _path = path;
        Icon = Icons.FileExplorerSegoe;
        Name = "Browse"; // TODO:LOC
        Title = path;
    }

    public override IListItem[] GetItems()
    {
        if (_directoryContents != null)
        {
            return _directoryContents.ToArray();
        }

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
        _directoryContents = contents
            .Select(s => new IndexerItem() { FullPath = s, FileName = Path.GetFileName(s) })
            .Select(i => new IndexerListItem(i))
            .ToList();

        _ = Task.Run(() =>
        {
            foreach (var item in _directoryContents)
            {
                IconInfo? icon = null;
                var stream = ThumbnailHelper.GetThumbnail(item.FilePath).Result;
                if (stream != null)
                {
                    var data = new IconData(RandomAccessStreamReference.CreateFromStream(stream));
                    icon = new IconInfo(data, data);
                    item.Icon = icon;
                }
            }
        });

        return _directoryContents.ToArray();
    }
}
