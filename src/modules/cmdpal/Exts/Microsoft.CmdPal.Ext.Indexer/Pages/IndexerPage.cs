// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CmdPal.Ext.Indexer.Data;
using Microsoft.CmdPal.Ext.Indexer.Indexer;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using WinRT;

namespace Microsoft.CmdPal.Ext.Indexer;

internal sealed partial class IndexerPage : DynamicListPage
{
    private readonly object _lockObject = new(); // Lock object for synchronization

    private readonly List<SearchResult> searchResults = new();

    private ISearchQuery searchQuery;

    private uint queryCookie = 10;

    public IndexerPage()
    {
        Icon = new("\ue729");
        Name = "Indexer";
    }

    public override ISection[] GetItems(string query)
    {
        var t = DoGetItems(query);
        t.ConfigureAwait(false);
        return t.Result;
    }

    public override ISection[] GetItems()
    {
        var t = DoGetItems(string.Empty);
        t.ConfigureAwait(false);
        return t.Result;
    }

    internal static readonly string[] FileTypeFilter = Array.Empty<string>();

    public async Task<uint> Query(string searchText)
    {
        queryCookie++;

        await Task.Run(() =>
        {
            lock (_lockObject)
            {
                if (searchQuery != null && !CanReuseQuery(searchQuery.QueryString, searchText))
                {
                    searchQuery.As<ISearchUXQuery>().CancelOutstandingQueries();
                    searchQuery = null;
                }

                if (searchQuery == null)
                {
                    // Create a new one, give it a new cookie, and put it into our map
                    searchQuery = new SearchUXQueryHelper().As<ISearchQuery>();
                    searchQuery.As<ISearchUXQuery>().Init();
                }

                // Just forward on to the helper with the right callback for feeding us results
                // Set up the binding for the items
                (searchQuery as ISearchUXQuery).Execute(searchText, queryCookie);
            }

            // unlock
            // Wait for the query executed event
            (searchQuery as ISearchUXQuery).WaitForQueryCompletedEvent();
        });

        if (searchText.Length > 0)
        {
            OnQueryCompleted();
        }
        else
        {
            // Just clear all the results from the UI
            searchResults.Clear();
        }

        return queryCookie;
    }

    private void OnQueryCompleted()
    {
        lock (_lockObject)
        {
            // race between drawing and selecting options...always check validity.
            if (searchQuery != null)
            {
                // Get the results from the query helper and stash in the UI
                /*var cookie = searchQuery.As<ISearchUXQuery>().Cookie;
                if (cookie != queryCookie)*/
                {
                    // If we are here, we are returning results on the same user input
                    var numResults = searchQuery.GetNumResults();
                    searchResults.Clear();

                    for (uint i = 0; i < numResults; i++)
                    {
                        searchResults.Add(searchQuery.As<ISearchUXQuery>().GetResult(i));
                    }
                }
            }
        }
    }

    private async Task<ISection[]> DoGetItems(string query)
    {
        var items = new List<IndexerItem>();
        await Task.Run(() => Query(query));

        if (searchResults == null)
        {
            return Array.Empty<ISection>();
        }

        foreach (var result in searchResults)
        {
            items.Add(new IndexerItem()
            {
                FileName = result.ItemDisplayName,
                FullPath = result.LaunchUri,
            });
        }

        var s = new ListSection()
        {
            Title = "Files",
            Items = items.Select((item) => new IndexerListItem(item)).ToArray(),
        };

        return [s];
    }

    private bool CanReuseQuery(string oldQuery, string newQuery)
    {
        if (newQuery.Length == 0 || oldQuery.Length > newQuery.Length)
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
        catch
        {
        }

        return false;
    }
}
