// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;

namespace Microsoft.CmdPal.UI;

public partial class LoadIconBehavior : DependencyObject, IBehavior
{
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
        DependencyProperty.Register("Source", typeof(IconDataType), typeof(LoadIconBehavior), new PropertyMetadata(new IconDataType(string.Empty)));

    public DependencyObject? AssociatedObject { get; private set; }

    public void Attach(DependencyObject associatedObject) => AssociatedObject = associatedObject;

    public void Detach() => AssociatedObject = null;

    public async void OnSourcePropertyChanged()
    {
        // TODO: Cache this in the class
        var iconService = App.Current.Services.GetService<IIconCacheService>()!;

        var icoSource = await iconService.GetIconSource(Source ?? new(string.Empty));

        if (AssociatedObject is Border border)
        {
            if (icoSource is FontIconSource fontIco)
            {
                IconSourceElement elem = new()
                {
                    IconSource = fontIco,
                };
                border.Child = elem;
            }
            else
            {
                var icoElement = icoSource?.CreateIconElement();
                border.Child = icoElement;
            }
        }
    }
}
