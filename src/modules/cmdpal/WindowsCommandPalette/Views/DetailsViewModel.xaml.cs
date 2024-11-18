// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Extensions;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace WindowsCommandPalette.Views;

public sealed class DetailsViewModel
{
    internal string Title { get; init; } = string.Empty;

    internal string Body { get; init; } = string.Empty;

    internal IconDataType HeroImage { get; init; } = new(string.Empty);

    internal async Task<ImageBrush?> IcoElement()
    {
        return await IconFromIconDataAsync(HeroImage); // Microsoft.Terminal.UI.IconPathConverter.IconMUX(HeroImage.Icon);
    }

    internal DetailsViewModel(IDetails details)
    {
        this.Title = details.Title;
        this.Body = details.Body;
        this.HeroImage = details.HeroImage ?? new(string.Empty);
    }

    public static async Task<ImageBrush?> IconFromIconDataAsync(IconDataType ico)
    {
        if (string.IsNullOrEmpty(ico.Icon) && ico.Data != null)
        {
            // var bitmapImage = new BitmapImage();
            // bitmapImage.DecodePixelWidth = 80;
            // var stream = await ico.Data.OpenReadAsync();
            // bitmapImage.SetSource(stream);
            // var icoElem = new ImageIconSource();

            // icoElem.ImageSource = bitmapImage;
            var image = new BitmapImage();
            using var bitmapStream = await ico.Data.OpenReadAsync();
            await image.SetSourceAsync(bitmapStream);
            var brush = new ImageBrush
            {
                ImageSource = image,
                Stretch = Stretch.Uniform,
            };

            // icoElem.Source = bitmapImage;
            return brush;

            // await bitmapSource.SetSourceAsync((Windows.Storage.Streams.IRandomAccessStream)ico.Data);
            // var icon = new ImageIconSource() { ImageSource = bitmapSource };
            // var elem = new IconSourceElement();
            // elem.IconSource = icon;
            // return elem;
        }
        else
        {
            return null;
        }
    }
}
