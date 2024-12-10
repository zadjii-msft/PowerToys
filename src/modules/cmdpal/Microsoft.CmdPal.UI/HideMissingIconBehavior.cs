// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.UI.ExtViews;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;

namespace Microsoft.CmdPal.UI;

public partial class HideMissingIconBehavior : DependencyObject, IBehavior
{
    private static readonly IconCacheService IconService = new(Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());

    public IconDataType Source
    {
        get => (IconDataType)GetValue(SourceProperty);
        set
        {
            SetValue(SourceProperty, value);
            OnSourcePropertyChanged();
        }
    }

    // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(nameof(Source), typeof(IconDataType), typeof(HideMissingIconBehavior), new PropertyMetadata(new IconDataType(string.Empty)));

    public DependencyObject? AssociatedObject { get; private set; }

    public void Attach(DependencyObject associatedObject) => AssociatedObject = associatedObject;

    public void Detach() => AssociatedObject = null;

    public void OnSourcePropertyChanged()
    {
        var showIcon = !string.IsNullOrEmpty(Source.Icon) || Source.Data != null;

        if (AssociatedObject is Border border)
        {
            border.Visibility = !showIcon ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
