﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using Microsoft.CmdPal.Ext.WindowWalker.Components;
using Microsoft.CmdPal.Ext.WindowWalker.Properties;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace Microsoft.CmdPal.Ext.WindowWalker.Pages;

internal sealed partial class WindowWalkerListPage : DynamicListPage, IDisposable
{
    private System.Threading.CancellationTokenSource _cancellationTokenSource = new();

    private bool _disposed;

    public WindowWalkerListPage()
    {
        Icon = new("\ue8f9"); // SwitchApps
        Name = Resources.windowwalker_name;
        Id = "com.microsoft.cmdpal.windowwalker";
        PlaceholderText = "Search open windows";
    }

    public override void UpdateSearchText(string oldSearch, string newSearch) =>
        RaiseItemsChanged(0);

    public List<WindowWalkerListItem> Query(string query)
    {
        ArgumentNullException.ThrowIfNull(query);

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new System.Threading.CancellationTokenSource();

        WindowWalkerCommandsProvider.VirtualDesktopHelperInstance.UpdateDesktopList();
        OpenWindows.Instance.UpdateOpenWindowsList(_cancellationTokenSource.Token);
        SearchController.Instance.UpdateSearchText(query);
        var searchControllerResults = SearchController.Instance.SearchMatches;

        return ResultHelper.GetResultList(searchControllerResults, !string.IsNullOrEmpty(query), "\uE946");
    }

    public override IListItem[] GetItems() => Query(SearchText).ToArray();

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _cancellationTokenSource?.Dispose();
                _disposed = true;
            }
        }
    }
}
