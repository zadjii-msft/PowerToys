// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using CommunityToolkit.Common;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.CmdPal.UI.ViewModels;
using Microsoft.CmdPal.UI.ViewModels.Messages;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Microsoft.CmdPal.UI;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ListPage : Page,
    IRecipient<NavigateNextCommand>,
    IRecipient<NavigatePreviousCommand>,
    IRecipient<ActivateSelectedListItemMessage>,
    IRecipient<ActivateSecondaryCommandMessage>
{
    private readonly DispatcherQueue _queue = DispatcherQueue.GetForCurrentThread();

    public ListViewModel? ViewModel
    {
        get => (ListViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    // Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(ListViewModel), typeof(ListPage), new PropertyMetadata(null, OnViewModelChanged));

    public ViewModelLoadedState LoadedState
    {
        get => (ViewModelLoadedState)GetValue(LoadedStateProperty);
        set => SetValue(LoadedStateProperty, value);
    }

    // Using a DependencyProperty as the backing store for LoadedState.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty LoadedStateProperty =
        DependencyProperty.Register(nameof(LoadedState), typeof(ViewModelLoadedState), typeof(ListPage), new PropertyMetadata(ViewModelLoadedState.Loading));

    public string ErrorMessage
    {
        get => (string)GetValue(ErrorMessageProperty);
        set => SetValue(ErrorMessageProperty, value);
    }

    // Using a DependencyProperty as the backing store for LoadedState.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ErrorMessageProperty =
        DependencyProperty.Register(nameof(ErrorMessage), typeof(string), typeof(ListPage), new PropertyMetadata(string.Empty));

    public ListPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        LoadedState = ViewModelLoadedState.Loading;
        if (e.Parameter is ListViewModel lvm)
        {
            if (!lvm.IsInitialized
                && lvm.InitializeCommand != null)
            {
                ViewModel = null;

                _ = Task.Run(async () => await InitializeViewmodel(lvm));
            }
            else
            {
                ViewModel = lvm;
                WeakReferenceMessenger.Default.Send<NavigateToPageMessage>(new(lvm));
                LoadedState = ViewModelLoadedState.Loaded;
            }
        }

        if (e.NavigationMode == NavigationMode.Back)
        {
            // Upon navigating _back_ to this page, immediately select the
            // first item in the list
            ItemsList.SelectedIndex = 0;
        }

        // RegisterAll isn't AOT compatible
        WeakReferenceMessenger.Default.Register<NavigateNextCommand>(this);
        WeakReferenceMessenger.Default.Register<NavigatePreviousCommand>(this);
        WeakReferenceMessenger.Default.Register<ActivateSelectedListItemMessage>(this);
        WeakReferenceMessenger.Default.Register<ActivateSecondaryCommandMessage>(this);

        base.OnNavigatedTo(e);
    }

    private async Task InitializeViewmodel(ListViewModel lvm)
    {
        // You know, this creates the situation where we wait for
        // both loading page properties, AND the items, before we
        // display anything.
        //
        // We almost need to do an async await on initialize, then
        // just a fire-and-forget on FetchItems.
        lvm.InitializeCommand.Execute(null);

        await lvm.InitializeCommand.ExecutionTask!;

        if (lvm.InitializeCommand.ExecutionTask.Status != TaskStatus.RanToCompletion)
        {
            // TODO: Handle failure case
            System.Diagnostics.Debug.WriteLine(lvm.InitializeCommand.ExecutionTask.Exception);

            // TODO GH #239 switch back when using the new MD text block
            // _ = _queue.EnqueueAsync(() =>
            _queue.TryEnqueue(new(() =>
            {
                LoadedState = ViewModelLoadedState.Error;
            }));
        }
        else
        {
            // TODO GH #239 switch back when using the new MD text block
            // _ = _queue.EnqueueAsync(() =>
            _queue.TryEnqueue(new(() =>
{
    var result = (bool)lvm.InitializeCommand.ExecutionTask.GetResultOrDefault()!;

    ViewModel = lvm;

    WeakReferenceMessenger.Default.Send<NavigateToPageMessage>(new(result ? lvm : null));
    LoadedState = result ? ViewModelLoadedState.Loaded : ViewModelLoadedState.Error;

    // Immediately select the first item in the list
    ItemsList.SelectedIndex = 0;
}));
        }
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        base.OnNavigatingFrom(e);

        WeakReferenceMessenger.Default.Unregister<NavigateNextCommand>(this);
        WeakReferenceMessenger.Default.Unregister<NavigatePreviousCommand>(this);
        WeakReferenceMessenger.Default.Unregister<ActivateSelectedListItemMessage>(this);
        WeakReferenceMessenger.Default.Unregister<ActivateSecondaryCommandMessage>(this);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "VS is too agressive at pruning methods bound in XAML")]
    private void ListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is ListItemViewModel item)
        {
            ViewModel?.InvokeItemCommand.Execute(item);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "VS is too agressive at pruning methods bound in XAML")]
    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Debug.WriteLine("ItemsList_SelectionChanged");
        // Debug.WriteLine($"  +{e.AddedItems.Count} / -{e.RemovedItems.Count}");
        // Debug.WriteLine($"  selected='{ItemsList.SelectedItem}'");
        if (ItemsList.SelectedItem is ListItemViewModel item)
        {
            ViewModel?.UpdateSelectedItemCommand.Execute(item);
        }

        // There's mysterious behavior here, where the selection seemingly
        // changes to _nothing_ when we're backspacing to a single charater.
        // And at that point, seemingly the item that's getting removed is not
        // a member of FilteredItems. Very bizarre.
        //
        // Might be able to fix in the future by stashing the removed item
        // here, then in Page_ItemsUpdated trying to select that cached item if
        // it's in the list (otherwise, clear the cache), but that seems
        // aggressively bodgy for something that mostly just works today.
    }

    public void Receive(NavigateNextCommand message)
    {
        // Note: We may want to just have the notion of a 'SelectedCommand' in our VM
        // And then have these commands manipulate that state being bound to the UI instead
        // We may want to see how other non-list UIs need to behave to make this decision
        // At least it's decoupled from the SearchBox now :)
        if (ItemsList.SelectedIndex < ItemsList.Items.Count - 1)
        {
            ItemsList.SelectedIndex++;
            ItemsList.ScrollIntoView(ItemsList.SelectedItem);
        }
    }

    public void Receive(NavigatePreviousCommand message)
    {
        if (ItemsList.SelectedIndex > 0)
        {
            ItemsList.SelectedIndex--;
            ItemsList.ScrollIntoView(ItemsList.SelectedItem);
        }
    }

    public void Receive(ActivateSelectedListItemMessage message)
    {
        if (ItemsList.SelectedItem is ListItemViewModel item)
        {
            ViewModel?.InvokeItemCommand.Execute(item);
        }
    }

    public void Receive(ActivateSecondaryCommandMessage message)
    {
        if (ItemsList.SelectedItem is ListItemViewModel item)
        {
            ViewModel?.InvokeSecondaryCommandCommand.Execute(item);
        }
    }

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ListPage @this)
        {
            if (e.OldValue is ListViewModel old)
            {
                old.PropertyChanged -= @this.ViewModel_PropertyChanged;
                old.ItemsUpdated -= @this.Page_ItemsUpdated;
            }

            if (e.NewValue is ListViewModel page)
            {
                page.PropertyChanged += @this.ViewModel_PropertyChanged;
                page.ItemsUpdated += @this.Page_ItemsUpdated;
            }
        }
    }

    // Called after we've finished updating the whole list for either a
    // GetItems or a change in the filter.
    private void Page_ItemsUpdated(ListViewModel sender, object args)
    {
        // If for some reason, we don't have a selected item, fix that.
        //
        // It's important to do this here, because once there's no selection
        // (which can happen as the list updates) we won't get an
        // ItemsList_SelectionChanged again to give us another chance to change
        // the selection from null -> something. Better to just update the
        // selection once, at the end of all the updating.
        if (ItemsList.SelectedItem == null)
        {
            ItemsList.SelectedIndex = 0;
        }
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        var prop = e.PropertyName;
        if (prop == nameof(ViewModel.ErrorMessage) && ViewModel != null)
        {
            if (!string.IsNullOrEmpty(ViewModel.ErrorMessage))
            {
                LoadedState = ViewModelLoadedState.Error;
            }
        }
        else if (prop == nameof(ViewModel.FilteredItems))
        {
            Debug.WriteLine($"ViewModel.FilteredItems {ItemsList.SelectedItem}");
        }
    }
}

public enum ViewModelLoadedState
{
    Loaded,
    Loading,
    Error,
}
