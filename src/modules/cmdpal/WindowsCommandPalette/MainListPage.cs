// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using WindowsCommandPalette.Models;
using WindowsCommandPalette.Views;

namespace WindowsCommandPalette;

public sealed partial class MainListPage : DynamicListPage
{
    private readonly MainViewModel _mainViewModel;

    private readonly FilteredListSection _filteredSection;
    private readonly ObservableCollection<MainListItem> topLevelItems = new();

    public MainListPage(MainViewModel viewModel)
    {
        this._mainViewModel = viewModel;

        // wacky: "All apps" is added to _mainViewModel.TopLevelCommands before
        // we're constructed, so we never get a
        // TopLevelCommands_CollectionChanged callback when we're first launched
        // that would let us add it
        foreach (var i in _mainViewModel.TopLevelCommands)
        {
            this.topLevelItems.Add(new MainListItem(i.Unsafe));
        }

        _filteredSection = new(_mainViewModel);
        _filteredSection.TopLevelItems = topLevelItems;
        _mainViewModel.TopLevelCommands.CollectionChanged += TopLevelCommands_CollectionChanged;

        PlaceholderText = "Search...";
        ShowDetails = true;
        Loading = false;
    }

    public override IListItem[] GetItems(string query)
    {
        _filteredSection.Query = query;

        var fallbacks = topLevelItems
            .Select(i => i?.FallbackHandler)
            .Where(fb => fb != null)
            .Select(fb => fb!);

        foreach (var fb in fallbacks)
        {
            fb.UpdateQuery(query);
        }

        if (string.IsNullOrEmpty(query))
        {
            return topLevelItems.ToArray();
        }
        else
        {
            return _filteredSection.Items;
        }
    }

    private void TopLevelCommands_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Debug.WriteLine("TopLevelCommands_CollectionChanged");
        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
        {
            foreach (var item in e.NewItems)
            {
                if (item is ExtensionObject<IListItem> listItem)
                {
                    topLevelItems.Add(new MainListItem(listItem.Unsafe));
                }
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
        {
            foreach (var item in e.OldItems)
            {
                if (item is ExtensionObject<IListItem> _)
                {
                    // foreach (var mainListItem in _mainSection.TopLevelItems)
                    // {
                    //    if (mainListItem.Item == listItem)
                    //    {
                    //        // _mainSection.TopLevelItems.Remove(mainListItem);
                    //        break;
                    //    }
                    // }
                }
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            // _mainSection.Reset();
            // _filteredSection.Reset();
            topLevelItems.Clear();
        }

        // _recentsListSection.Reset();
        // topLevelItems.Clear();
        this.OnPropertyChanged("Items");
    }
}
