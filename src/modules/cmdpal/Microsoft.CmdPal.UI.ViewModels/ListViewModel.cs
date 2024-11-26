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

    public ListViewModel(IListPage model)
        : base(model)
    {
        _model = new(model);

        // TODO: probably need to make async here
        _ = Task.Run(InitializePropertiesAsync);

        // Initialize();
    }

    private void Model_ItemsChanged(object sender, ItemsChangedEventArgs args) => FetchItems();

    private void FetchItems()
    {
        Task.Factory.StartNew(
           () =>
       {
           // TEMPORARY: just plop all the items into a single group
           // see 9806fe5d8 for the last commit that had this with sections
           ObservableGroup<string, ListItemViewModel> group = new(string.Empty);

           // TODO unsafe
           var newItems = _model.Unsafe!.GetItems();
           Items.Clear();
           foreach (var item in newItems)
           {
               ListItemViewModel viewModel = new(item);
               _ = viewModel.InitializePropertiesAsync();
               group.Add(viewModel);
           }

           Items.AddGroup(group);
       },
           CancellationToken.None,
           TaskCreationOptions.None,
           Scheduler).Wait();
    }

    // InvokeItemCommand is what this will be in Xaml due to source generator
    [RelayCommand]
    private void InvokeItem(ListItemViewModel item) => WeakReferenceMessenger.Default.Send<PerformCommandMessage>(new(item.Command));

    [RelayCommand]
    private void UpdateSelectedItem(ListItemViewModel item) => WeakReferenceMessenger.Default.Send<UpdateActionBarMessage>(new(item));

    protected override void Initialize()
    {
        base.Initialize();

        var listPage = _model.Unsafe;
        if (listPage == null)
        {
            return; // throw?
        }

        FetchItems();
        listPage.ItemsChanged += Model_ItemsChanged;
    }
}
