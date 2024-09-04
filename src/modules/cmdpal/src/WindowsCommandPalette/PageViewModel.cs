// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Dispatching;
using Microsoft.Windows.CommandPalette.Extensions;
using Windows.Foundation;

namespace DeveloperCommandPalette;

public class SubmitFormArgs
{
    public required string FormData { get; set; }

    public required IForm Form { get; set; }
}

public class PageViewModel : INotifyPropertyChanged
{
    private readonly DispatcherQueue dispatcherQueue;

    private bool nested;

    public bool Nested { get => nested; set => nested = value; }

    private bool loading;

    public bool Loading
    {
        get => loading;
        set
        {
            loading = value;
            BubbleXamlPropertyChanged(nameof(PlaceholderText));
        }
    }

    public string PlaceholderText => Loading ? "Loading..." : "Type here to search";

    protected IPage pageAction { get; }

    //public IPage PageAction { get => pageAction; set => pageAction = value; }
    public ActionViewModel Command { get; }

    public event TypedEventHandler<object, ActionViewModel>? RequestDoAction;

    public event TypedEventHandler<object, SubmitFormArgs>? RequestSubmitForm;

    public event TypedEventHandler<object, object>? RequestGoBack;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected PageViewModel(IPage page)
    {
        page.PropChanged += Page_PropertyChanged;
        this.Loading = page.Loading;
        this.pageAction = page;
        this.Command = new(page);

        this.dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    }

    private void Page_PropertyChanged(object sender, Microsoft.Windows.CommandPalette.Extensions.PropChangedEventArgs args)
    {
        switch (args.PropertyName)
        {
            case "Loading":
                {
                    this.Loading = pageAction.Loading;
                    break;
                }
        }

        BubbleXamlPropertyChanged(args.PropertyName);

    }

    private void BubbleXamlPropertyChanged(string propertyName)
    {
        if (this.dispatcherQueue == null)
        {
            // this is highly unusual
            return;
        }
        this.dispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
        {
            this.PropertyChanged?.Invoke(this, new(propertyName));
        });
    }

    public void DoAction(ActionViewModel action)
    {
        var handlers = RequestDoAction;
        handlers?.Invoke(this, action);
    }

    public void GoBack()
    {
        var handlers = RequestGoBack;
        handlers?.Invoke(this, new());
    }

    public void SubmitForm(string formData, IForm form)
    {
        var handlers = RequestSubmitForm;
        handlers?.Invoke(this, new() { FormData = formData, Form = form });
    }
}
