// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Microsoft.CmdPal.UI.ViewModels;
using Microsoft.CmdPal.UI.ViewModels.MainPage;
using Microsoft.CmdPal.UI.ViewModels.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue;

namespace Microsoft.CmdPal.UI;

public partial class DetailsDataTemplateSelector : DataTemplateSelector
{
    // Define the (currently empty) data templates to return
    // These will be "filled-in" in the XAML code.
    public required DataTemplate LinkTemplate { get; set; }

    public required DataTemplate SeparatorTemplate { get; set; }

    public required DataTemplate TagTemplate { get; set; }

    protected override DataTemplate? SelectTemplateCore(object item)
    {
        if (item is DetailsElementViewModel element)
        {
            var data = element.Data;
            return data switch
            {
                IDetailsSeparator => SeparatorTemplate,
                IDetailsLink => LinkTemplate,
                IDetailsTags => TagTemplate,
                _ => null,
            };
        }

        return null;
    }
}
