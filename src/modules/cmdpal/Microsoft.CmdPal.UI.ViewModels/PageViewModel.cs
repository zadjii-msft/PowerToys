// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    public partial bool Loading { get; private set; } = true;

    [ObservableProperty]
    public partial bool IsInitialized { get; private set; }

    public PageViewModel(IPage model)
    {
        _pageModel = new(model);
        Scheduler = TaskScheduler.FromCurrentSynchronizationContext();
    }

    //// Run on background thread from ListPage.xaml.cs
    [RelayCommand]
    private Task<bool> InitializeAsync()
    {
        // TODO: We may want a SemaphoreSlim lock here.

        // TODO: We may want to investigate using some sort of AsyncEnumerable or populating these as they come in to the UI layer
        //       Though we have to think about threading here and circling back to the UI thread with a TaskScheduler.
        InitializeProperties();

        IsInitialized = true;
        return Task.FromResult(true);
    }

    public override void InitializeProperties()
    {
        var page = _pageModel.Unsafe;
        if (page == null)
        {
            return; // throw?
        }

        Name = page.Name;
        Loading = page.Loading;

        page.PropChanged += Model_PropChanged;
    }

    private void Model_PropChanged(object sender, PropChangedEventArgs args)
    {
        try
        {
            var propName = args.PropertyName;
            FetchProperty(propName);

            OnPropertyChanged(propName);
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
                var newName = model.Name ?? string.Empty;
                Task.Factory.StartNew(() => { this.Name = newName; }, CancellationToken.None, TaskCreationOptions.None, Scheduler);

                break;
            case nameof(Loading):
                // this.Loading = model.Loading;
                var newLoading = model.Loading;
                Task.Factory.StartNew(() => { this.Loading = newLoading; }, CancellationToken.None, TaskCreationOptions.None, Scheduler);
                break;

                // TODO! Icon
        }
    }
}
