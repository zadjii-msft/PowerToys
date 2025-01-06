// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.UI.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;

namespace Microsoft.CmdPal.UI.Controls;

public partial class Tag : Control
{
    public OptionalColor? BackgroundColor
    {
        get => (OptionalColor?)GetValue(BackgroundColorProperty);
        set => SetValue(BackgroundColorProperty, value);
    }

    public OptionalColor? ForegroundColor
    {
        get => (OptionalColor?)GetValue(ForegroundColorProperty);
        set => SetValue(ForegroundColorProperty, value);
    }

    public OptionalColor? BorderBrushColor
    {
        get => (OptionalColor?)GetValue(BorderBrushColorProperty);
        set => SetValue(BorderBrushColorProperty, value);
    }

    public bool? HasIcon
    {
        get => (bool?)GetValue(HasIconProperty);
        set => SetValue(HasIconProperty, value);
    }

    public IconDataType? Icon
    {
        get => (IconDataType?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly DependencyProperty ForegroundColorProperty =
        DependencyProperty.Register(nameof(ForegroundColor), typeof(OptionalColor), typeof(Tag), new PropertyMetadata(null, OnForegroundColorPropertyChanged));

    public static readonly DependencyProperty BackgroundColorProperty =
        DependencyProperty.Register(nameof(BackgroundColor), typeof(OptionalColor), typeof(Tag), new PropertyMetadata(null, OnBackgroundColorPropertyChanged));

    public static readonly DependencyProperty BorderBrushColorProperty =
    DependencyProperty.Register(nameof(BorderBrushColor), typeof(OptionalColor), typeof(Tag), new PropertyMetadata(null, OnBorderColorPropertyChanged));

    public static readonly DependencyProperty HasIconProperty =
    DependencyProperty.Register(nameof(HasIcon), typeof(bool), typeof(Tag), new PropertyMetadata(null));

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(IconDataType), typeof(Tag), new PropertyMetadata(null));

    public static readonly DependencyProperty TextProperty =
    DependencyProperty.Register(nameof(Text), typeof(string), typeof(Tag), new PropertyMetadata(null));

    public Tag()
    {
        this.DefaultStyleKey = typeof(Tag);
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
    }

    private static void OnForegroundColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Tag tag || tag.ForegroundColor is null || !tag.ForegroundColor.HasValue)
        {
            return;
        }

        if (OptionalColorBrushCacheProvider.Convert(tag.ForegroundColor.Value) is SolidColorBrush brush)
        {
            tag.Foreground = brush;
        }
    }

    private static void OnBackgroundColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Tag tag || tag.BackgroundColor is null || !tag.BackgroundColor.HasValue)
        {
            return;
        }

        if (OptionalColorBrushCacheProvider.Convert(tag.BackgroundColor.Value) is SolidColorBrush brush)
        {
            tag.Background = brush;
        }
    }

    private static void OnBorderColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Tag tag || tag.BorderBrushColor is null || !tag.BorderBrushColor.HasValue)
        {
            return;
        }

        if (OptionalColorBrushCacheProvider.Convert(tag.BorderBrushColor.Value) is SolidColorBrush brush)
        {
            tag.BorderBrush = brush;
        }
    }
}
