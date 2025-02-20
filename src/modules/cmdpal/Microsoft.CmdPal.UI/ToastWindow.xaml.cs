// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.WinUI;
using Microsoft.CmdPal.UI.ViewModels;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;

namespace Microsoft.CmdPal.UI;

public sealed partial class ToastWindow : Window
{
    public ToastViewModel ViewModel { get; } = new();

    private readonly DispatcherQueueTimer _debounceTimer = DispatcherQueue.GetForCurrentThread().CreateTimer();

    public ToastWindow()
    {
        this.InitializeComponent();
        AppWindow.Hide();
        AppWindow.IsShownInSwitchers = false;
        ExtendsContentIntoTitleBar = true;
        AppWindow.SetPresenter(AppWindowPresenterKind.CompactOverlay);
        AppWindow.SetIcon("ms-appx:///Assets/Icons/StoreLogo.png");
        AppWindow.Title = "Command Palette Settings";
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Collapsed;

        // PositionCentered();
    }

    private void PositionCentered()
    {
        // AppWindow.Resize(new SizeInt32 { Width = 1280, Height = 32 });

        // var floatSize = ToastText.ActualSize.ToSize();
        var intSize = new SizeInt32 { Width = Convert.ToInt32(ToastText.ActualWidth), Height = Convert.ToInt32(ToastText.ActualHeight) };
        AppWindow.Resize(intSize);

        var displayArea = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Nearest);
        if (displayArea is not null)
        {
            var centeredPosition = AppWindow.Position;
            centeredPosition.X = (displayArea.WorkArea.Width - AppWindow.Size.Width) / 2;

            var monitorHeight = displayArea.WorkArea.Height;
            var windowHeight = AppWindow.Size.Height;
            centeredPosition.Y = (int)(monitorHeight - (windowHeight * 2));
            AppWindow.Move(centeredPosition);
        }
    }

    public void ShowToast(string message)
    {
        ViewModel.ToastMessage = message;
        DispatcherQueue.TryEnqueue(
            DispatcherQueuePriority.Low,
            () =>
        {
            PositionCentered();
            AppWindow.Show();

            _debounceTimer.Debounce(
                () =>
                {
                    AppWindow.Hide();
                },
                interval: TimeSpan.FromMilliseconds(2500),
                immediate: false);
        });
    }
}
