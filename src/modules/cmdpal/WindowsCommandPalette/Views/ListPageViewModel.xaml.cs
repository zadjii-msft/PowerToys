// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Microsoft.UI.Dispatching;

namespace WindowsCommandPalette.Views;

public sealed class ListPageViewModel : PageViewModel
{
    private readonly ObservableCollection<SectionInfoList> _items = [];

    public ObservableCollection<SectionInfoList> FilteredItems { get; set; } = [];

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
            this.UpdateListItems().ConfigureAwait(false);
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
        ObservableCollection<SectionInfoList> newItems = new();

        // foreach (var item in items)
        // {
        //    newItems.Add(new ListItemViewModel(item));
        // }
        SectionInfoList s = new(string.Empty, items.Select(i => new ListItemViewModel(i)));
        newItems.Add(s);
        ListHelpers.InPlaceUpdateList(_items, newItems);
        ListHelpers.InPlaceUpdateList(FilteredItems, newItems);
    }

    internal async Task<Collection<SectionInfoList>> GetFilteredItems(string query)
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

            //// TODO! Probably bad that this turns list view models into listitems back to NEW view models
            // return ListHelpers.FilterList(Items.Select(vm => vm.ListItem), Query).Select(li => new ListItemViewModel(li)).ToList();
            try
            {
                var allFilteredItems = ListHelpers.FilterList(
                    _items
                        .SelectMany(section => section)
                        .Select(vm => vm.ListItem.Unsafe),
                    _query).Select(li => new ListItemViewModel(li));

                var newSection = new SectionInfoList(string.Empty, allFilteredItems);
                return [newSection];
            }
            catch (COMException ex)
            {
                return [new SectionInfoList(string.Empty, [new ListItemViewModel(new ErrorListItem(ex))])];
            }
        }
    }
}
