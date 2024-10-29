﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.CmdPal.Extensions;
using Microsoft.UI.Dispatching;

namespace WindowsCommandPalette.Views;

public class SectionInfoList : ObservableCollection<ListItemViewModel>
{
    public string Title { get; }

    private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    public SectionInfoList(string title, IEnumerable<ListItemViewModel> items)
        : base(items)
    {
        Title = title;
        if (items is INotifyCollectionChanged observable)
        {
            observable.CollectionChanged -= Items_CollectionChanged;
            observable.CollectionChanged += Items_CollectionChanged;
        }

        if (this._dispatcherQueue == null)
        {
            throw new InvalidOperationException("DispatcherQueue is null");
        }
    }

    private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // DispatcherQueue.TryEnqueue(() => {
        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
        {
            foreach (var i in e.NewItems)
            {
                if (i is IListItem li)
                {
                    if (!string.IsNullOrEmpty(li.Title))
                    {
                        ListItemViewModel vm = new(li);
                        this.Add(vm);
                    }

                    // if (isDynamic)
                    // {
                    //    // Dynamic lists are in charge of their own
                    //    // filtering. They know if this thing was already
                    //    // filtered or not.
                    //    FilteredItems.Add(vm);
                    // }
                }
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            this.Clear();

            // Items.Clear();
            // if (isDynamic)
            // {
            //    FilteredItems.Clear();
            // }
        }

        // });
    }
}
