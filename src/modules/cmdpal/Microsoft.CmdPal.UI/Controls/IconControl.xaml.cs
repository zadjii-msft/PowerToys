// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.UI.ExtViews;
using Microsoft.CmdPal.UI.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.CmdPal.UI.Controls;

public sealed partial class IconControl : UserControl
{
    private static readonly IconCacheService IconService = new(Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());

    private IconViewModel? _viewModel;

    public IconViewModel? ViewModel { get => _viewModel; set => AttachViewModel(value); }

    public IconControl()
    {
        this.InitializeComponent();
    }

    private void AttachViewModel(IconViewModel? vm)
    {
        // if (_viewModel != null)
        // {
        //     _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        // }
        _viewModel = vm;

        if (_viewModel != null)
        {
            DispatcherQueue.TryEnqueue(async () =>
            {
                var icoSource = await IconService.GetIconSource(_viewModel);

                if (icoSource is FontIconSource fontIco)
                {
                    if (!double.IsNaN(this.Width))
                    {
                        fontIco.FontSize = this.Width;
                    }

                    // For inexplicable reasons, FontIconSource.CreateIconElement
                    // doesn't work, so do it ourselves
                    IconSourceElement elem = new()
                    {
                        IconSource = fontIco,
                    };
                    IconBorder.Child = elem;
                }
                else
                {
                    var icoElement = icoSource?.CreateIconElement();
                    IconBorder.Child = icoElement;
                }
            });
        }
        else
        {
            IconBorder.Child = null;
        }
    }
}
