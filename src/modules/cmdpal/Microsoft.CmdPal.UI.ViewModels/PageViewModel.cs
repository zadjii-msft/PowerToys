// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.UI.ViewModels.Models;

namespace Microsoft.CmdPal.UI.ViewModels;

public partial class PageViewModel : ExtensionObjectViewModel
{
    protected TaskScheduler Scheduler { get; private set; }

    private readonly ExtensionObject<IPage> _pageModel;

    [ObservableProperty]
    public partial string Name { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial bool Loading { get; private set; } = false;

    public PageViewModel(IPage model)
    {
        _pageModel = new(model);
        Scheduler = TaskScheduler.FromCurrentSynchronizationContext();
        model.PropChanged += Model_PropChanged;
    }

    protected override void Initialize()
    {
        var page = _pageModel.Unsafe;
        if (page == null)
        {
            return; // throw?
        }

        Name = page.Name;
        Loading = page.Loading;
    }

    private void Model_PropChanged(object sender, PropChangedEventArgs args)
    {
        try
        {
            FetchProperty(args.PropertyName);
            OnPropertyChanged(args.PropertyName);
        }
        catch (Exception)
        {
            // TODO log? throw?
        }
    }

    protected virtual void FetchProperty(string propertyName)
    {
        var model = this._pageModel.Unsafe;
        if (model == null)
        {
            return; // throw?
        }

        switch (propertyName)
        {
            case nameof(Name):
                this.Name = model.Name ?? string.Empty;
                break;
            case nameof(Loading):
                this.Loading = model.Loading;
                break;

                // TODO! Icon
        }
    }
}
