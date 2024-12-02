// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.CmdPal.Ext.Indexer.Indexer.Propsys;
using Microsoft.CmdPal.Ext.Indexer.Indexer.Utils;
using Microsoft.CmdPal.Ext.Indexer.Utils;

namespace Microsoft.CmdPal.Ext.Indexer.Indexer;

internal sealed class SearchQuery : SearchQueryBase, IDisposable
{
    private readonly Lock _lockObject = new(); // Lock object for synchronization
    private readonly List<SearchResult> searchResults = [];
    private const uint QueryTimerThreshold = 85;
    private EventWaitHandle queryCompletedEvent;
    private Timer queryTpTimer;

    public bool ContentSearchEnabled { get; private set; }

    public uint Cookie { get; set; }

    public string SearchText { get; private set; }

    public string QueryString { get; set; }

    public SearchQuery()
    {
    }

    public void Init()
    {
        // Create all the objects we will want cached
        try
        {
            queryTpTimer = new Timer(QueryTimerCallback, this, Timeout.Infinite, Timeout.Infinite);
            if (queryTpTimer == null)
            {
                Logger.LogError("Failed to create query timer");
                return;
            }

            queryCompletedEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
            if (queryCompletedEvent == null)
            {
                Logger.LogError("Failed to create query completed event");
                return;
            }

            // Execute a synchronous query on file/mapi items to prime the index and keep that handle around
            PrimeIndexAndCacheWhereId();
        }
        catch (Exception ex)
        {
            Logger.LogError("Exception at SearchUXQueryHelper Init", ex);
        }
    }

    public uint GetNumResults() => NumResults;

    public SearchResult GetResult(uint idx) => idx >= searchResults.Count ? throw new ArgumentOutOfRangeException(nameof(idx)) : searchResults[(int)idx];

    public void WaitForQueryCompletedEvent() => queryCompletedEvent.WaitOne();

    public void CancelOutstandingQueries()
    {
        Logger.LogDebug("Cancel query " + SearchText);

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

        Init();
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
                var fireTime = DateTime.UtcNow.AddMilliseconds(QueryTimerThreshold);
                var dueTime = fireTime - DateTime.UtcNow;
                queryTpTimer.Change(dueTime, Timeout.InfiniteTimeSpan);
            }
        }
    }

    public static void QueryTimerCallback(object state)
    {
        Logger.LogDebug($"QueryTimerCallback: {state}");

        var pQueryHelper = (SearchQuery)state;
        pQueryHelper.ExecuteSyncInternal();
    }

    // If we've gotten this far we have successful results...only now clear the result list and update it
    public override void OnPreFetchRows() => searchResults.Clear();

    public override void OnPostFetchRows()
    {
        NumResults = (uint)searchResults.Count; // num results is really how many we display
        queryCompletedEvent.Set();
    }

    public override void OnFetchRowCallback(IPropertyStore propStore) => CreateSearchResult(propStore);

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
        catch (Exception ex)
        {
            Logger.LogError("Exception at SearchUXQueryHelper ExecuteSyncInternal", ex);
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
            Logger.LogError("Get item name display error: " + hr);
            return;
        }

        // Get item URL
        PROPVARIANT itemUrl;
        PROPERTYKEY pKeyItemUrl = new() { fmtid = new System.Guid("49691C90-7E17-101A-A91C-08002B2ECDA9"), pid = 9 };
        hr = propStore.GetValue(ref pKeyItemUrl, out itemUrl);
        if (hr != 0)
        {
            Logger.LogError("Get item URL error: " + hr);
            return;
        }

        // Get kind text
        PROPVARIANT kindText;
        PROPERTYKEY pKeyKindText = new() { fmtid = new System.Guid("F04BEF95-C585-4197-A2B7-DF46FDC9EE6D"), pid = 100 };
        hr = propStore.GetValue(ref pKeyKindText, out kindText);
        if (hr != 0)
        {
            Logger.LogError("Get kind text error: " + hr);
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

        searchResults.Add(searchResult);

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
            result = result[(indexProtocolFound + fileProtocolString.Length)..];
        }

        return result;
    }

    public void Dispose()
    {
        CancelOutstandingQueries();
        queryCompletedEvent?.Dispose();
    }
}
