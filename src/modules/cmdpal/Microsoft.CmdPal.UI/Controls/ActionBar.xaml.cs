// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.Messaging;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.UI.ViewModels;
using Microsoft.CmdPal.UI.ViewModels.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Microsoft.CmdPal.UI.Controls;

public sealed partial class ActionBar : UserControl,
    IRecipient<UpdateActionBarPage>
{
    public ActionBarViewModel ViewModel { get; set; } = new();

    public ActionBar()
    {
        this.InitializeComponent();

        WeakReferenceMessenger.Default.Register<UpdateActionBarPage>(this);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "VS has a tendency to delete XAML bound methods over-agressively")]
    private void ActionListViewItem_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        // TODO
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "VS has a tendency to delete XAML bound methods over-agressively")]
    private void ActionListViewItem_Tapped(object sender, TappedRoutedEventArgs e)
    {
        MoreCommandsButton.Flyout.Hide();

        if (sender is not ListViewItem listItem)
        {
            return;
        }

        if (listItem.DataContext is CommandContextItemViewModel item)
        {
            ViewModel?.InvokeItemCommand.Execute(item);
        }
    }

    public void Receive(UpdateActionBarPage message)
    {
        if (ViewModel.CurrentPage != null)
        {
            // ViewModel.CurrentPage.PropertyChanged -= CurrentPage_PropertyChanged;
        }

        ViewModel.CurrentPage = message.Page;

        if (ViewModel.CurrentPage != null)
        {
            // ViewModel.CurrentPage.PropertyChanged += CurrentPage_PropertyChanged;
        }

        // ViewModel.CurrentPage.PropertyChanged += CurrentPage_PropertyChanged;
    }

    private void CurrentPage_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (ViewModel == null)
        {
            return;
        }

        if (e.PropertyName == nameof(ViewModel.CurrentPage.Icon))
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                var iconService = App.Current.Services.GetService<IIconCacheService>()!;
                var icoSource = await iconService.GetIconSource(ViewModel.CurrentPage?.Icon ?? new(string.Empty));
                if (icoSource != null)
                {
                    this.IconBorder.Child = icoSource.CreateIconElement();
                }
            });
        }
    }

    public async Task<UIElement?> FetchIcon(IconDataType? ico)
    {
        var iconService = App.Current.Services.GetService<IIconCacheService>()!;
        var icoSource = await iconService.GetIconSource(ico ?? new(string.Empty));
        return icoSource?.CreateIconElement();
    }
}
