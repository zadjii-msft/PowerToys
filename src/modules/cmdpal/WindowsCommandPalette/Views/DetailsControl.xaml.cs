// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Shapes;

namespace WindowsCommandPalette.Views;

public sealed partial class DetailsControl : UserControl
{
    public DetailsViewModel ViewModel { get; set; }

    public DetailsControl(DetailsViewModel vm)
    {
        ViewModel = vm;
        InitializeComponent();
        UpdateHeroImageIcon();
    }

    private async void UpdateHeroImageIcon()
    {
        var brush = await ViewModel.IcoElement();
        if (brush != null)
        {
            Rectangle r = new();
            r.Fill = brush;
            r.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch;
            r.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Stretch;
            r.Height = this.ActualWidth;
            r.Width = this.ActualWidth;

            // HeroImageIcon.IconSource = source;
            HeroIconContent.Content = r;
        }
    }
}
