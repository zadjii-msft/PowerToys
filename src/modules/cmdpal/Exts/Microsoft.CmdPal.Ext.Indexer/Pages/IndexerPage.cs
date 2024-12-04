// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.CmdPal.Ext.Indexer.Data;
using Microsoft.CmdPal.Ext.Indexer.Indexer;
using Microsoft.CmdPal.Ext.Indexer.Utils;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace Microsoft.CmdPal.Ext.Indexer;

internal sealed partial class IndexerPage : DynamicListPage, IDisposable
{
    private readonly Lock _lockObject = new(); // Lock object for synchronization

    private SearchQuery _searchQuery = new();

    private uint _queryCookie = 10;

    public IndexerPage()
    {
        Icon = new("\ue729");
        Name = "Indexer";
        PlaceholderText = "Search for files and folders...";

        _searchQuery.Init();
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        if (oldSearch != newSearch)
        {
            Logger.LogDebug($"Update {oldSearch} -> {newSearch}");
            RaiseItemsChanged(0);
        }
    }

    public override IListItem[] GetItems() => DoGetItems(SearchText);

    private IListItem[] DoGetItems(string query)
    {
        if (query == string.Empty)
        {
            return [];
        }

        var items = new List<IndexerListItem>();

        Stopwatch stopwatch = new();
        stopwatch.Start();
        Query(query);

        lock (_lockObject)
        {
            if (_searchQuery != null)
            {
                var cookie = _searchQuery.Cookie;
                if (cookie == _queryCookie)
                {
                    foreach (var result in _searchQuery.SearchResults)
                    {
                        items.Add(new IndexerListItem(new IndexerItem
                        {
                            FileName = result.ItemDisplayName,
                            FullPath = result.LaunchUri,
                        })
                        {
                            Icon = new(result.IsFolder ? "\uE838" : "\uE8E5"),
                        });
                    }
                }
            }
        }

        stopwatch.Stop();
        Logger.LogDebug($"Query time: {stopwatch.ElapsedMilliseconds} ms, results: {items.Count}, query: {query}");

        return [.. items];
    }

    private uint Query(string searchText)
    {
        if (searchText == string.Empty)
        {
            return _queryCookie;
        }

        _queryCookie++;
        lock (_lockObject)
        {
            if (!CanReuseQuery(_searchQuery.QueryString, searchText))
            {
                _searchQuery.CancelOutstandingQueries();
            }

            // Just forward on to the helper with the right callback for feeding us results
            // Set up the binding for the items
            _searchQuery.Execute(searchText, _queryCookie);
        }

        // unlock
        // Wait for the query executed event
        _searchQuery.WaitForQueryCompletedEvent();

        return _queryCookie;
    }

    private bool CanReuseQuery(string oldQuery, string newQuery)
    {
        if (newQuery.Length == 0 || oldQuery == null || oldQuery.Length > newQuery.Length)
        {
            return false;
        }
        else if (oldQuery.Length == 0)
        {
            Logger.LogInfo("CanReuseQuery: oldQuery is empty");
            return true;
        }

        try
        {
            var oldQueryLower = oldQuery.ToLower(CultureInfo.CurrentCulture);
            var newQueryLower = newQuery.ToLower(CultureInfo.CurrentCulture);

            var isPrefix = oldQueryLower.Zip(newQueryLower, (c1, c2) => c1 == c2)
                        .TakeWhile(match => match)
                        .Count() == oldQueryLower.Length;

            if (isPrefix)
            {
                Logger.LogInfo("CanReuseQuery: isPrefix");
                return true;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("CanReuseQuery exception", ex);
        }

        return false;
    }

    public void Dispose()
    {
        _searchQuery = null;
        GC.SuppressFinalize(this);
    }
}
