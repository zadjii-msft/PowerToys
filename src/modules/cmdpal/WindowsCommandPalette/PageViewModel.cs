// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel;
using Microsoft.CmdPal.Extensions;
using Microsoft.UI.Dispatching;
using Windows.Foundation;

namespace WindowsCommandPalette;

public class PageViewModel : INotifyPropertyChanged
{
    private readonly DispatcherQueue dispatcherQueue;

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool Nested { get; set; }

    public IPage PageAction { get; }

    private bool items;

    public bool Items
    {
        get => items;
        set
        {
            items = value;
            BubbleXamlPropertyChanged(nameof(items));
        }
    }

    // public IPage PageAction { get => pageAction; set => pageAction = value; }
    public ActionViewModel Command { get; }

    public event TypedEventHandler<object, ActionViewModel>? RequestDoAction;

    public event TypedEventHandler<object, SubmitFormArgs>? RequestSubmitForm;

    public event TypedEventHandler<object, object>? RequestGoBack;

    protected PageViewModel(IPage page)
    {
        PageAction = page;
        Command = new(page);

        page.PropChanged += Page_PropertyChanged;
        this.dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    }

    private void Page_PropertyChanged(object sender, PropChangedEventArgs args)
    {
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
