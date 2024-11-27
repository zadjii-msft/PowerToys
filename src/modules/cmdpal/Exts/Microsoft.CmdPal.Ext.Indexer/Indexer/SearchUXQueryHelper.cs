// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.CmdPal.Ext.Indexer.Indexer.Propsys;
using Microsoft.CmdPal.Ext.Indexer.Indexer.Utils;

namespace Microsoft.CmdPal.Ext.Indexer.Indexer;

internal sealed class SearchUXQueryHelper : SearchQueryBase, ISearchQuery, ISearchUXQuery, IDisposable
{
    private readonly object _lockObject = new(); // Lock object for synchronization
    private readonly List<SearchResult> searchResults = new();
    private const uint QueryTimerThreshold = 85;
    private EventWaitHandle queryCompletedEvent;
    private Timer queryTpTimer;

    public bool ContentSearchEnabled { get; private set; }

    public uint Cookie { get; set; }

    public string SearchText { get; private set; }

    public string QueryString { get; set; }

    public SearchUXQueryHelper()
    {
    }

    // ISearchUXQuery
    public void Init()
    {
        // Create all the objects we will want cached
        try
        {
            queryTpTimer = new Timer(QueryTimerCallback, this, Timeout.Infinite, Timeout.Infinite);
            if (queryTpTimer == null)
            {
                // TODO : Log error
                return;
            }

            queryCompletedEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
            if (queryCompletedEvent == null)
            {
                // TODO : Log error
                return;
            }

            // Execute a synchronous query on file/mapi items to prime the index and keep that handle around
            PrimeIndexAndCacheWhereId();
        }
        catch
        {
            // Log error
        }
    }

    public uint GetNumResults()
    {
        return NumResults;
    }

    public SearchResult GetResult(uint idx)
    {
        if (idx >= searchResults.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(idx));
        }

        return searchResults[(int)idx];
    }

    public void WaitForQueryCompletedEvent()
    {
        queryCompletedEvent.WaitOne();
    }

    public void CancelOutstandingQueries()
    {
        // Are we currently doing work? If so, let's cancel
        lock (_lockObject)
        {
            if (queryTpTimer != null)
            {
                queryTpTimer.Change(Timeout.Infinite, Timeout.Infinite);
                queryTpTimer.Dispose();
                queryTpTimer = null;
            }
        }
    }

    public void Execute(string searchText, uint cookie)
    {
        lock (_lockObject)
        {
            if (queryTpTimer != null)
            {
                // We cancel the outstanding query callback and queue a new one every time
                queryTpTimer.Change(Timeout.Infinite, Timeout.Infinite);
                SearchText = searchText;
                Cookie = cookie;

                // Queue query
                DateTime fireTime = DateTime.UtcNow.AddMilliseconds(QueryTimerThreshold);
                TimeSpan dueTime = fireTime - DateTime.UtcNow;
                queryTpTimer.Change(dueTime, Timeout.InfiniteTimeSpan);
            }
        }
    }

    // ISearchQuery
    public void ExecuteSync()
    {
    }

    // Other public methods
    public static void QueryTimerCallback(object state)
    {
        SearchUXQueryHelper pQueryHelper = (SearchUXQueryHelper)state;
        pQueryHelper.ExecuteSyncInternal();
    }

    public override void OnPreFetchRows()
    {
        // If we've gotten this far we have successful results...only now clear the result list and update it
        searchResults.Clear();
    }

    public override void OnPostFetchRows()
    {
        NumResults = (uint)searchResults.Count; // num results is really how many we display
        queryCompletedEvent.Set();
    }

    public override void OnFetchRowCallback(IPropertyStore propStore)
    {
        CreateSearchResult(propStore);
    }

    public override string GetPrimingQueryString()
    {
        var builder = new QueryStringBuilder();
        var queryStr = builder.GeneratePrimingQuery();
        return queryStr;
    }

    private void ExecuteSyncInternal()
    {
        try
        {
            var builder = new QueryStringBuilder();
            var queryStr = builder.GenerateQuery(SearchText, ReuseWhereID);
            ExecuteQueryStringSync(queryStr);
        }
        catch
        {
            // TODO: log error
        }
    }

    private void CreateSearchResult(IPropertyStore propStore)
    {
        // Get item name display
        PROPVARIANT itemNameDisplay;
        PROPERTYKEY pKeyItemNameDisplay = new() { fmtid = new System.Guid("B725F130-47EF-101A-A5F1-02608C9EEBAC"), pid = 10 };
        var hr = propStore.GetValue(ref pKeyItemNameDisplay, out itemNameDisplay);
        if (hr != 0)
        {
            // TODO: log error
            return;
        }

        // Get item URL
        PROPVARIANT itemUrl;
        PROPERTYKEY pKeyItemUrl = new() { fmtid = new System.Guid("49691C90-7E17-101A-A91C-08002B2ECDA9"), pid = 9 };
        hr = propStore.GetValue(ref pKeyItemUrl, out itemUrl);
        if (hr != 0)
        {
            // TODO: log error
            return;
        }

        // Get kind text
        PROPVARIANT kindText;
        PROPERTYKEY pKeyKindText = new() { fmtid = new System.Guid("F04BEF95-C585-4197-A2B7-DF46FDC9EE6D"), pid = 100 };
        hr = propStore.GetValue(ref pKeyKindText, out kindText);
        if (hr != 0)
        {
            // TODO: log error
            return;
        }

        var isFolder = false;
        if (kindText.vt == (ushort)VarEnum.VT_LPWSTR && kindText.unionValue.pwszVal != IntPtr.Zero)
        {
            var kindString = Marshal.PtrToStringUni(kindText.unionValue.pwszVal);
            if (string.Equals(kindString, "Folder", StringComparison.OrdinalIgnoreCase))
            {
                isFolder = true;
            }
        }

        var filePath = Marshal.PtrToStringUni(itemUrl.unionValue.pwszVal);
        if (filePath == null)
        {
            return;
        }

        filePath = ConvertUrlToFilePath(filePath);

        // Create the actual result object
        var searchResult = new SearchResult(
            Marshal.PtrToStringUni(itemNameDisplay.unionValue.pwszVal),
            Marshal.PtrToStringUni(itemUrl.unionValue.pwszVal),
            filePath,
            isFolder);

        // if (searchResult.CanDisplay)
        {
            searchResults.Add(searchResult);
        }

        itemUrl.Dispose();
        kindText.Dispose();
    }

    private string ConvertUrlToFilePath(string url)
    {
        var result = url.Replace('/', '\\'); // replace all '/' to '\\'

        var fileProtocolString = "file:";
        var indexProtocolFound = url.IndexOf(fileProtocolString, StringComparison.CurrentCultureIgnoreCase);

        if (indexProtocolFound != -1 && (indexProtocolFound + fileProtocolString.Length) < url.Length)
        {
            result = result.Substring(indexProtocolFound + fileProtocolString.Length);
        }

        return result;
    }

    public void Dispose()
    {
        CancelOutstandingQueries();
        queryCompletedEvent?.Dispose();
    }
}
