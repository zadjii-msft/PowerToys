// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using Microsoft.CmdPal.Ext.Apps.Programs;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.CmdPal.UI.ViewModels.MainPage;

/// <summary>
/// This class encapsulates the data we load from built-in providers and extensions to use within the same extension-UI system for a <see cref="ListPage"/>.
/// TODO: Need to think about how we structure/interop for the page -> section -> item between the main setup, the extensions, and our viewmodels.
/// </summary>
public partial class MainListPage : DynamicListPage
{
    private readonly IServiceProvider _serviceProvider;

    private readonly ObservableCollection<TopLevelCommandWrapper> _commands;

    private IEnumerable<IListItem>? _filteredItems;

    private bool _appsLoading = true;

    public MainListPage(IServiceProvider serviceProvider)
    {
        Name = "Command Palette";

        _serviceProvider = serviceProvider;

        var tlcManager = _serviceProvider.GetService<TopLevelCommandManager>()!;
        tlcManager.PropertyChanged += TlcManager_PropertyChanged;

        // reference the TLC collection directly... maybe? TODO is this a good idea ot a terrible one?
        _commands = tlcManager.TopLevelCommands;
        _commands.CollectionChanged += Commands_CollectionChanged;

        // This _should_ start a background thread to start loading all the apps.
        // It _feels_ like it does it's work on the main thread though, like it
        // slows down startup... which doesn't make sense.
        _ = Task.Run(() =>
        {
            _ = AppCache.Instance.Value;
            _appsLoading = false;
            Debug.WriteLine("Loaded apps");
            IsLoading = ActuallyLoading();
        });
    }

    private void TlcManager_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IsLoading))
        {
            IsLoading = ActuallyLoading();
        }
    }

    private void Commands_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => RaiseItemsChanged(_commands.Count);

    public override IListItem[] GetItems()
    {
        Debug.WriteLine("main GetItems");
        return string.IsNullOrEmpty(SearchText)
            ? _commands.Select(tlc => tlc).ToArray()
            : _filteredItems?.ToArray() ?? [];
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        /* handle changes to the filter text here */
        IEnumerable<IListItem> commands = _commands;
        IEnumerable<IListItem> apps = AllAppsCommandProvider.Page.GetItems();
        var allItems = commands.Concat(apps);
        _filteredItems = ListHelpers.FilterList(allItems, SearchText);
        RaiseItemsChanged(_filteredItems.Count());
    }

    private bool ActuallyLoading()
    {
        var tlcManager = _serviceProvider.GetService<TopLevelCommandManager>()!;
        return _appsLoading || tlcManager.IsLoading;
    }
}
