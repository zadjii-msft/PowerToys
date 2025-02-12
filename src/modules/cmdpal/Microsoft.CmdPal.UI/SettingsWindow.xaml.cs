// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.Messaging;
using Microsoft.CmdPal.UI.Pages;
using Microsoft.CmdPal.UI.ViewModels;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;

namespace Microsoft.CmdPal.UI;

public sealed partial class SettingsWindow : Window,
    IRecipient<NavigateToExtensionSettingsMessage>
{
    public SettingsWindow()
    {
        this.InitializeComponent();
        this.ExtendsContentIntoTitleBar = true;
        this.AppWindow.SetIcon("ms-appx:///Assets/Icons/StoreLogo.png");
        this.AppWindow.Title = "Command Palette Settings";
        this.AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
        PositionCentered();
        WeakReferenceMessenger.Default.Register<NavigateToExtensionSettingsMessage>(this);
    }

    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        NavView.SelectedItem = NavView.MenuItems[0];
        Navigate("General");
    }

    private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        var selectedItem = args.InvokedItem;

        if (selectedItem is not null)
        {
            Navigate(selectedItem.ToString()!);
        }
    }

    private void Navigate(string page)
    {
        var pageType = page switch
        {
            "General" => typeof(GeneralPage),
            "Extensions" => typeof(ExtensionsPage),
            _ => null,
        };
        if (pageType is not null)
        {
            NavFrame.Navigate(pageType);
        }
    }

    private void Navigate(ProviderSettingsViewModel extension) => NavFrame.Navigate(typeof(ExtensionPage), extension);

    private void PositionCentered()
    {
        AppWindow.Resize(new SizeInt32 { Width = 1280, Height = 720 });
        var displayArea = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Nearest);
        if (displayArea is not null)
        {
            var centeredPosition = AppWindow.Position;
            centeredPosition.X = (displayArea.WorkArea.Width - AppWindow.Size.Width) / 2;
            centeredPosition.Y = (displayArea.WorkArea.Height - AppWindow.Size.Height) / 2;
            AppWindow.Move(centeredPosition);
        }
    }

    public void Receive(NavigateToExtensionSettingsMessage message) => Navigate(message.ProviderSettingsVM);
}
