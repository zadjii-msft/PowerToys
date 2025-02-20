// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;

namespace Microsoft.CmdPal.UI;

public sealed partial class ToastWindow : Window
{
    public ToastWindow()
    {
        this.InitializeComponent();
        this.ExtendsContentIntoTitleBar = true;
        this.AppWindow.Hide();
        this.AppWindow.IsShownInSwitchers = false;
        this.AppWindow.SetIcon("ms-appx:///Assets/Icons/StoreLogo.png");
        this.AppWindow.Title = "Command Palette Settings";
        this.AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Collapsed;
        PositionCentered();
    }

    private void PositionCentered()
    {
        AppWindow.Resize(new SizeInt32 { Width = 1280, Height = 32 });
        var displayArea = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Nearest);
        if (displayArea is not null)
        {
            var centeredPosition = AppWindow.Position;
            centeredPosition.X = (displayArea.WorkArea.Width - AppWindow.Size.Width) / 2;
            centeredPosition.Y = (int)((displayArea.WorkArea.Height - AppWindow.Size.Height) * .75);
            AppWindow.Move(centeredPosition);
        }
    }
}
