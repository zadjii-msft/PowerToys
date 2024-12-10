﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.UI.ViewModels.Messages;
using Microsoft.CmdPal.UI.ViewModels.Models;

namespace Microsoft.CmdPal.UI.ViewModels;

public partial class ListViewModel : PageViewModel
{
    // Observable from MVVM Toolkit will auto create public properties that use INotifyPropertyChange change
    // https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/observablegroupedcollections for grouping support
    [ObservableProperty]
    public partial ObservableGroupedCollection<string, ListItemViewModel> Items { get; set; } = [];

    private readonly ExtensionObject<IListPage> _model;

    // Remember - "observable" properties from the model (via PropChanged)
    // cannot be marked [ObservableProperty]
    public bool ShowDetails { get; private set; }

    public string PlaceholderText { get => string.IsNullOrEmpty(field) ? "Type here to search..." : field; private set; } = string.Empty;

    public ListViewModel(IListPage model, TaskScheduler scheduler)
        : base(model, scheduler)
    {
        _model = new(model);
    }

    private void Model_ItemsChanged(object sender, ItemsChangedEventArgs args) => FetchItems();

    //// Run on background thread, from InitializeAsync or Model_ItemsChanged
    private void FetchItems()
    {
        ObservableGroup<string, ListItemViewModel> group = new(string.Empty);

        // TEMPORARY: just plop all the items into a single group
        // see 9806fe5d8 for the last commit that had this with sections
        // TODO unsafe
        try
        {
            var newItems = _model.Unsafe!.GetItems();

            Items.Clear();

            foreach (var item in newItems)
            {
                ListItemViewModel viewModel = new(item, Scheduler);
                viewModel.InitializeProperties();
                group.Add(viewModel);
            }

            // Am I really allowed to modify that observable collection on a BG
            // thread and have it just work in the UI??
            Items.AddGroup(group);
        }
        catch (Exception ex)
        {
            ShowException(ex);
            throw;
        }
    }

    // InvokeItemCommand is what this will be in Xaml due to source generator
    [RelayCommand]
    private void InvokeItem(ListItemViewModel item) => WeakReferenceMessenger.Default.Send<PerformCommandMessage>(new(item.Command));

    [RelayCommand]
    private void UpdateSelectedItem(ListItemViewModel item)
    {
        WeakReferenceMessenger.Default.Send<UpdateActionBarMessage>(new(item));

        if (ShowDetails && item.HasDetails)
        {
            WeakReferenceMessenger.Default.Send<ShowDetailsMessage>(new(item.Details));
        }
        else
        {
            WeakReferenceMessenger.Default.Send<HideDetailsMessage>();
        }
    }

    public override void InitializeProperties()
    {
        base.InitializeProperties();

        var listPage = _model.Unsafe;
        if (listPage == null)
        {
            return; // throw?
        }

        ShowDetails = listPage.ShowDetails;
        UpdateProperty(nameof(ShowDetails));
        PlaceholderText = listPage.PlaceholderText;
        UpdateProperty(nameof(PlaceholderText));

        FetchItems();
        listPage.ItemsChanged += Model_ItemsChanged;
    }

    protected override void FetchProperty(string propertyName)
    {
        base.FetchProperty(propertyName);

        var model = this._model.Unsafe;
        if (model == null)
        {
            return; // throw?
        }

        switch (propertyName)
        {
            case nameof(ShowDetails):
                this.ShowDetails = model.ShowDetails;
                break;
            case nameof(PlaceholderText):
                this.PlaceholderText = model.PlaceholderText;
                break;
        }

        UpdateProperty(propertyName);
    }
}
