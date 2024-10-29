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

    // private readonly MainListSection _mainSection;
    // private readonly RecentsListSection _recentsListSection;
    private readonly FilteredListSection _filteredSection;
    private readonly ObservableCollection<MainListItem> topLevelItems = new();

    public MainListPage(MainViewModel viewModel)
    {
        this._mainViewModel = viewModel;

        // _mainSection = new(_mainViewModel);
        // _recentsListSection = new(_mainViewModel);
        _filteredSection = new(_mainViewModel);
        _filteredSection.TopLevelItems = topLevelItems;
        _mainViewModel.TopLevelCommands.CollectionChanged += TopLevelCommands_CollectionChanged;

        // _sections = [
        //     _recentsListSection,
        //     _mainSection
        // ];
        PlaceholderText = "Search...";
        ShowDetails = true;
        Loading = false;
    }

    public override IListItem[] GetItems(string query)
    {
        _filteredSection.Query = query;
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
