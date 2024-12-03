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

    public string Name { get; private set; } = string.Empty;

    // This is our visible loading state.
    // * The field stores the value from the IPage from the extension itself.
    // * The accessor returns a combination of the pages requested loading
    //   state, and if we're currently fetching content from the extension.
    public bool Loading { get => field || FetchingContent; private set; } = true;

    // Use this to track when we're still retrieving content from an extension.
    protected bool FetchingContent { get; set; }

    [ObservableProperty]
    public partial bool IsInitialized { get; private set; }

    public PageViewModel(IPage model, TaskScheduler scheduler)
    {
        _pageModel = new(model);
        Scheduler = scheduler;
    }

    //// Run on background thread from ListPage.xaml.cs
    [RelayCommand]
    private Task<bool> InitializeAsync()
    {
        // TODO: We may want a SemaphoreSlim lock here.

        // TODO: We may want to investigate using some sort of AsyncEnumerable or populating these as they come in to the UI layer
        //       Though we have to think about threading here and circling back to the UI thread with a TaskScheduler.
        try
        {
            InitializeProperties();
        }
        catch (Exception)
        {
            return Task.FromResult(false);
        }

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
        }

        UpdateProperty(propertyName);
    }

    protected void UpdateProperty(string propertyName) => Task.Factory.StartNew(() => { OnPropertyChanged(propertyName); }, CancellationToken.None, TaskCreationOptions.None, Scheduler);
}
