// Copyright (c) Microsoft Corporation
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

    public ListViewModel(IListPage model, TaskScheduler scheduler)
        : base(model, scheduler)
    {
        _model = new(model);
    }

    private void Model_ItemsChanged(object sender, ItemsChangedEventArgs args) => FetchItems();

    //// Run on background thread, from InitializeAsync or Model_ItemsChanged
    private void FetchItems()
    {
        // TEMPORARY: just plop all the items into a single group
        // see 9806fe5d8 for the last commit that had this with sections
        // TODO unsafe
        IListItem[]? newItems = null;
        List<ListItemViewModel> viewModels = [];

        // Internally start tracking that we're fetching content, and let the
        // UI know to start displaying the loading bar.
        FetchingContent = true;
        UpdateProperty(nameof(Loading));

        try
        {
            newItems = _model.Unsafe!.GetItems();
            foreach (var item in newItems)
            {
                ListItemViewModel viewModel = new(item, Scheduler);
                viewModel.InitializeProperties();
                viewModels.Add(viewModel);
            }
        }
        catch (Exception ex)
        {
            ShowException(ex);
            throw;
        }

        // Now hop onto the UI thread to update the actual list.
        Task.Factory.StartNew(
            () =>
            {
                FetchingContent = false;
                OnPropertyChanged(nameof(Loading));

                ObservableGroup<string, ListItemViewModel> group = new(string.Empty, viewModels);

                Items.Clear();
                Items.AddGroup(group);
            },
            CancellationToken.None,
            TaskCreationOptions.None,
            Scheduler);
    }

    // InvokeItemCommand is what this will be in Xaml due to source generator
    [RelayCommand]
    private void InvokeItem(ListItemViewModel item) => WeakReferenceMessenger.Default.Send<PerformCommandMessage>(new(item.Command));

    [RelayCommand]
    private void UpdateSelectedItem(ListItemViewModel item) => WeakReferenceMessenger.Default.Send<UpdateActionBarMessage>(new(item));

    public override void InitializeProperties()
    {
        base.InitializeProperties();

        var listPage = _model.Unsafe;
        if (listPage == null)
        {
            return; // throw?
        }

        // Start a _new_ background task to fetch the items. This will allow us
        // to update the UI in response to the basic page properties loading,
        // THEN fetch all the items from the extension (rather than wait till
        // all the items are loaded to display the page title)
        _ = Task.Run(() =>
        {
            FetchItems();
            listPage.ItemsChanged += Model_ItemsChanged;
        });
    }
}
