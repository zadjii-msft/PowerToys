// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using DeveloperCommandPalette;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace WindowsCommandPalette.Views;

public sealed partial class TagControl : UserControl
{
    public TagViewModel ViewModel { get; set; }

    public TagControl(TagViewModel vm)
    {
        ViewModel = vm;
        InitializeComponent();
    }
}
