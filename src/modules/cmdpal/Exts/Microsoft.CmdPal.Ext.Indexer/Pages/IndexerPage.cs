// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CmdPal.Ext.Indexer.Data;
using Microsoft.CmdPal.Ext.Indexer.Indexer;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using WinRT;

namespace Microsoft.CmdPal.Ext.Indexer;

internal sealed partial class IndexerPage : DynamicListPage
{
    private readonly Lock _lockObject = new(); // Lock object for synchronization

    private readonly List<SearchResult> _searchResults = [];

    private ISearchQuery _searchQuery;

    private uint _queryCookie = 10;

    public IndexerPage()
    {
        Icon = new("\ue729");
        Name = "Indexer";
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        if (string.IsNullOrEmpty(newSearch))
        {
            _searchResults.Clear();
        }

        if (oldSearch != newSearch)
        {
            RaiseItemsChanged(0);
        }
    }

    public override IListItem[] GetItems()
    {
        var t = DoGetItems(SearchText);
        t.ConfigureAwait(false);
        return t.Result;
    }

    private async Task<IListItem[]> DoGetItems(string query)
    {
        var items = new List<IndexerListItem>();
        await Task.Run(() => Query(query));

        lock (_lockObject)
        {
            // race between drawing and selecting options...always check validity.
            if (_searchQuery != null)
            {
                // Get the results from the query helper and stash in the UI
                var cookie = _searchQuery.As<ISearchUXQuery>().Cookie;
                if (cookie != _queryCookie)
                {
                    // If we are here, we are returning results on the same user input
                    var numResults = _searchQuery.GetNumResults();
                    _searchResults.Clear();

                    for (uint i = 0; i < numResults; i++)
                    {
                        _searchResults.Add(_searchQuery.As<ISearchUXQuery>().GetResult(i));
                    }
                }
            }

            if (_searchResults != null)
            {
                foreach (var result in _searchResults)
                {
                    items.Add(new IndexerListItem(new IndexerItem
                    {
                        FileName = result.ItemDisplayName,
                        FullPath = result.LaunchUri,
                    }));
                }
            }
        }

        return [.. items];
    }

    internal static readonly string[] FileTypeFilter = [];

    private async Task<uint> Query(string searchText)
    {
        if (searchText == string.Empty)
        {
            _searchResults.Clear();
            return _queryCookie;
        }

        _queryCookie++;
        await Task.Run(() =>
        {
            lock (_lockObject)
            {
                if (_searchQuery != null && !CanReuseQuery(_searchQuery.QueryString, searchText))
                {
                    _searchQuery.As<ISearchUXQuery>().CancelOutstandingQueries();
                    _searchQuery = null;
                }

                if (_searchQuery == null)
                {
                    // Create a new one, give it a new cookie, and put it into our map
                    _searchQuery = new SearchUXQueryHelper().As<ISearchQuery>();
                    _searchQuery.As<ISearchUXQuery>().Init();
                }

                // Just forward on to the helper with the right callback for feeding us results
                // Set up the binding for the items
                (_searchQuery as ISearchUXQuery).Execute(searchText, _queryCookie);
            }

            // unlock
            // Wait for the query executed event
            (_searchQuery as ISearchUXQuery).WaitForQueryCompletedEvent();
        });

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
                return true;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("CanReuseQuery exception", ex);
        }

        return false;
    }
}
