// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Common;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.UI.ViewModels;
using Microsoft.CmdPal.UI.ViewModels.MainPage;
using Microsoft.CmdPal.UI.ViewModels.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.System;
using DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue;

namespace Microsoft.CmdPal.UI;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ShellPage :
    Page,
    IRecipient<NavigateBackMessage>,
    IRecipient<NavigateToDetailsMessage>,
    IRecipient<PerformCommandMessage>,
    IRecipient<ShowDetailsMessage>,
    IRecipient<HideDetailsMessage>,
    IRecipient<LaunchUriMessage>
{
    private readonly DispatcherQueue _queue = DispatcherQueue.GetForCurrentThread();

    private readonly DrillInNavigationTransitionInfo _drillInNavigationTransitionInfo = new();

    private readonly SlideNavigationTransitionInfo _slideRightTransition = new() { Effect = SlideNavigationTransitionEffect.FromRight };

    public ShellViewModel ViewModel { get; private set; } = App.Current.Services.GetService<ShellViewModel>()!;

    public ShellPage()
    {
        this.InitializeComponent();

        // how we are doing navigation around
        WeakReferenceMessenger.Default.Register<NavigateBackMessage>(this);
        WeakReferenceMessenger.Default.Register<NavigateToDetailsMessage>(this);
        WeakReferenceMessenger.Default.Register<PerformCommandMessage>(this);

        WeakReferenceMessenger.Default.Register<ShowDetailsMessage>(this);
        WeakReferenceMessenger.Default.Register<HideDetailsMessage>(this);

        WeakReferenceMessenger.Default.Register<LaunchUriMessage>(this);

        RootFrame.Navigate(typeof(LoadingPage), ViewModel);
    }

    public void Receive(NavigateToDetailsMessage message) => RootFrame.Navigate(typeof(ListDetailPage), message.ListItem, _drillInNavigationTransitionInfo);

    public void Receive(NavigateBackMessage message)
    {
        if (RootFrame.CanGoBack)
        {
            HideDetails();

            RootFrame.GoBack();
            HideDetails();
            RootFrame.ForwardStack.Clear();
            SearchBox.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
        }
        else
        {
            // If we can't go back then we must be at the top and thus escape again should quit.
            WeakReferenceMessenger.Default.Send<DismissMessage>();
        }
    }

    public void Receive(PerformCommandMessage message)
    {
        var command = message.Command.Unsafe;
        if (command == null)
        {
            return;
        }

        // TODO: Actually loading up the page, or invoking the command -
        // that might belong in the model, not the view?
        // Especially considering the try/catch concerns around the fact that the
        // COM call might just fail.
        // Or the command may be a stub. Future us problem.
        try
        {
            // This could be navigation to another page or invoking of a command, those are our two main branches of logic here.
            // For different pages, we may construct different view models and navigate to the central frame to different pages,
            // Otherwise the logic is mostly the same, outside the main page.
            if (command is IPage page)
            {
                _ = DispatcherQueue.TryEnqueue(() =>
                {
                    // Also hide our details pane about here, if we had one
                    HideDetails();

                    // Construct our ViewModel of the appropriate type and pass it the UI Thread context.
                    PageViewModel pageViewModel = page switch
                    {
                        IListPage listPage => new ListViewModel(listPage, TaskScheduler.FromCurrentSynchronizationContext()),
                        IFormPage formsPage => new FormsPageViewModel(formsPage, TaskScheduler.FromCurrentSynchronizationContext()),
                        IMarkdownPage markdownPage => new MarkdownPageViewModel(markdownPage, TaskScheduler.FromCurrentSynchronizationContext()),
                        _ => throw new NotSupportedException(),
                    };

                    // Kick off async loading of our ViewModel
                    ViewModel.LoadPageViewModel(pageViewModel);

                    // Navigate to the appropriate host page for that VM
                    RootFrame.Navigate(
                        page switch
                        {
                            IListPage => typeof(ListPage),
                            IFormPage => typeof(FormsPage),
                            IMarkdownPage => typeof(MarkdownPage),
                            _ => throw new NotSupportedException(),
                        },
                        pageViewModel,
                        _slideRightTransition);

                    // Refocus on the Search for continual typing on the next search request
                    SearchBox.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);

                    if (command is MainListPage)
                    {
                        // todo bodgy
                        RootFrame.BackStack.Clear();
                    }

                    // Set our page back in the ViewModel
                    // Note, this shortcuts and fights a bit with our LoadPageViewModel above, but we want to better fast display and incrementally load anyway
                    // We just need to reconcile our loading systems a bit more in the future.
                    ViewModel.CurrentPage = pageViewModel;
                });
            }
            else if (command is IInvokableCommand invokable)
            {
                // TODO Handle results
                _ = invokable.Invoke();
            }
        }
        catch (Exception ex)
        {
            if (command is IPageContext page)
            {
                page.ShowException(ex);
            }
            else
            {
                // TODO: Logging
            }
        }
    }

    public void Receive(ShowDetailsMessage message)
    {
        ViewModel.Details = message.Details;
        ViewModel.IsDetailsVisible = true;
    }

    public void Receive(HideDetailsMessage message) => HideDetails();

    private void HideDetails() => ViewModel.IsDetailsVisible = false;

    public void Receive(LaunchUriMessage message) => _ = Launcher.LaunchUriAsync(message.Uri);
}
