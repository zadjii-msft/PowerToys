// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Microsoft.UI.Dispatching;

namespace WindowsCommandPalette.Views;

public sealed class ListPageViewModel : PageViewModel
{
    private readonly ObservableCollection<ListItemViewModel> _items = [];

    public ObservableCollection<ListItemViewModel> FilteredItems { get; set; } = [];

    internal IListPage Page => (IListPage)this.PageAction;

    private bool IsDynamic => Page is IDynamicListPage;

    private IDynamicListPage? IsDynamicPage => Page as IDynamicListPage;

    private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    private string _query = string.Empty;

    public ListPageViewModel(IListPage page)
        : base(page)
    {
        page.PropChanged += Page_PropChanged;
    }

    private void Page_PropChanged(object sender, PropChangedEventArgs args)
    {
        if (args.PropertyName == "Items")
        {
            Debug.WriteLine("Items changed");
            _ = this.UpdateListItems();
        }
    }

    internal Task InitialRender()
    {
        return UpdateListItems();
    }

    internal async Task UpdateListItems()
    {
        // on main thread
        var t = new Task<IListItem[]>(() =>
        {
            try
            {
                return IsDynamicPage != null ?
                    IsDynamicPage.GetItems(_query) :
                    this.Page.GetItems();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return [new ErrorListItem(ex)];
            }
        });
        t.Start();
        var items = await t;

        // still on main thread

        // TODO! For dynamic lists, we're clearing out the whole list of items
        // we already have, then rebuilding it. We shouldn't do that. We should
        // still use the results from GetItems and put them into the code in
        // UpdateFilter to intelligently add/remove as needed.
        // Items.Clear();
        // FilteredItems.Clear();
        // ObservableCollection<SectionInfoList> newItems = new();

        // foreach (var item in items)
        // {
        //    newItems.Add(new ListItemViewModel(item));
        // }
        // SectionInfoList s = new(string.Empty, items.Select(i => new ListItemViewModel(i)));
        // newItems.Add(s);
        Collection<ListItemViewModel> newItems = new(items.Select(i => new ListItemViewModel(i)).ToList());
        Debug.WriteLine($"  Found {newItems.Count} items");

        ListHelpers.InPlaceUpdateList(FilteredItems, newItems);
        ListHelpers.InPlaceUpdateList(_items, newItems);

        /*_items.Clear();
        FilteredItems.Clear();
        foreach (var i in newItems)
        {
            FilteredItems.Add(i);
            _items.Add(i);
        }*/

        Debug.WriteLine($"Done with UpdateListItems, found {FilteredItems.Count} / {_items.Count}");
    }

    internal async Task<IEnumerable<ListItemViewModel>> GetFilteredItems(string query)
    {
        if (query == _query)
        {
            return FilteredItems;
        }

        _query = query;
        if (IsDynamic)
        {
            await UpdateListItems();
            return FilteredItems;
        }
        else
        {
            // Static lists don't need to re-fetch the items
            if (string.IsNullOrEmpty(query))
            {
                return _items;
            }

            // TODO! Probably bad that this turns list view models into listitems back to NEW view models
            // TODO! make this safer
            var newFilter = ListHelpers
                .FilterList(_items.Select(vm => vm.ListItem.Unsafe), query)
                .Select(li => new ListItemViewModel(li));

            // ListHelpers.InPlaceUpdateList(FilteredItems, new(newFilter.ToList()));
            return newFilter;
        }
    }
}
