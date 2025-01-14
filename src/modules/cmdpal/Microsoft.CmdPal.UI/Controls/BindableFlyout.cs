// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace Microsoft.CmdPal.UI.Controls;

public class BindableFlyout : DependencyObject
{
    public static IEnumerable? GetItemsSource(DependencyObject obj)
    {
        return obj.GetValue(ItemsSourceProperty) as IEnumerable;
    }

    public static void SetItemsSource(DependencyObject obj, IEnumerable value)
    {
        obj.SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.RegisterAttached(
            "ItemsSource",
            typeof(IEnumerable),
            typeof(BindableFlyout),
            new PropertyMetadata(null, ItemsSourceChanged));

    private static void ItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        Setup((Microsoft.UI.Xaml.Controls.Flyout)d);
    }

    public static DataTemplate GetItemTemplate(DependencyObject obj)
    {
        return (DataTemplate)obj.GetValue(ItemTemplateProperty);
    }

    public static void SetItemTemplate(DependencyObject obj, DataTemplate value)
    {
        obj.SetValue(ItemTemplateProperty, value);
    }

    public static readonly DependencyProperty ItemTemplateProperty =
        DependencyProperty.RegisterAttached(
            "ItemTemplate",
            typeof(DataTemplate),
            typeof(BindableFlyout),
            new PropertyMetadata(null, ItemsTemplateChanged));

    private static void ItemsTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        Setup((Microsoft.UI.Xaml.Controls.Flyout)d);
    }

    private static void Setup(Microsoft.UI.Xaml.Controls.Flyout m)
    {
        // if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
        // {
        //    return;
        // }
        var s = GetItemsSource(m);
        if (s == null)
        {
            return;
        }

        var t = GetItemTemplate(m);
        if (t == null)
        {
            return;
        }

        var c = new Microsoft.UI.Xaml.Controls.ItemsControl
        {
            ItemsSource = s,
            ItemTemplate = t,
        };

        var n = DispatcherQueuePriority.Normal;

        // Windows.UI.Core.DispatchedHandler h =;
        m.DispatcherQueue.TryEnqueue(n, () => m.Content = c);

        // await m.Dispatcher.RunAsync(n, () => m.Content = c);
    }
}
