// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel;
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
    IRecipient<PerformCommandMessage>,
    IRecipient<ShowDetailsMessage>,
    IRecipient<HideDetailsMessage>,
    IRecipient<ClearSearchMessage>,
    IRecipient<HandleCommandResultMessage>,
    IRecipient<LaunchUriMessage>,
    INotifyPropertyChanged
{
    private readonly DispatcherQueue _queue = DispatcherQueue.GetForCurrentThread();

    private readonly SlideNavigationTransitionInfo _slideRightTransition = new() { Effect = SlideNavigationTransitionEffect.FromRight };

    public ShellViewModel ViewModel { get; private set; } = App.Current.Services.GetService<ShellViewModel>()!;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ShellPage()
    {
        this.InitializeComponent();

        // how we are doing navigation around
        WeakReferenceMessenger.Default.Register<NavigateBackMessage>(this);
        WeakReferenceMessenger.Default.Register<PerformCommandMessage>(this);
        WeakReferenceMessenger.Default.Register<HandleCommandResultMessage>(this);

        WeakReferenceMessenger.Default.Register<ShowDetailsMessage>(this);
        WeakReferenceMessenger.Default.Register<HideDetailsMessage>(this);

        WeakReferenceMessenger.Default.Register<ClearSearchMessage>(this);
        WeakReferenceMessenger.Default.Register<LaunchUriMessage>(this);

        RootFrame.Navigate(typeof(LoadingPage), ViewModel);
    }

    public void Receive(NavigateBackMessage message)
    {
        if (RootFrame.CanGoBack)
        {
            GoBack();
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
                _ = _queue.TryEnqueue(() =>
                {
                    // Also hide our details pane about here, if we had one
                    HideDetails();

                    var isMainPage = command is MainListPage;

                    // Construct our ViewModel of the appropriate type and pass it the UI Thread context.
                    PageViewModel pageViewModel = page switch
                    {
                        IListPage listPage => new ListViewModel(listPage, TaskScheduler.FromCurrentSynchronizationContext())
                        {
                            IsNested = !isMainPage,
                        },
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

                    if (isMainPage)
                    {
                        // todo bodgy
                        RootFrame.BackStack.Clear();
                    }

                    // Note: Originally we set our page back in the ViewModel here, but that now happens in response to the Frame navigating triggered from the above
                    // See RootFrame_Navigated event handler.
                });
            }
            else if (command is IInvokableCommand invokable)
            {
                // TODO Handle results
                var result = invokable.Invoke();
                HandleCommandResult(result);
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

    private void HandleCommandResult(ICommandResult? result)
    {
        try
        {
            if (result != null)
            {
                var kind = result.Kind;
                switch (kind)
                {
                    case CommandResultKind.Dismiss:
                        {
                            // Reset the palette to the main page and dismiss
                            GoHome();
                            WeakReferenceMessenger.Default.Send<DismissMessage>();
                            break;
                        }

                    case CommandResultKind.GoHome:
                        {
                            // Go back to the main page, but keep it open
                            GoHome();
                            break;
                        }

                    case CommandResultKind.GoBack:
                        {
                            GoBack();
                            break;
                        }

                    case CommandResultKind.Hide:
                        {
                            // Keep this page open, but hide the palette.
                            WeakReferenceMessenger.Default.Send<DismissMessage>();

                            break;
                        }

                    case CommandResultKind.KeepOpen:
                        {
                            // Do nothing.
                            break;
                        }
                }
            }
        }
        catch
        {
        }
    }

    public void Receive(ShowDetailsMessage message)
    {
        ViewModel.Details = message.Details;
        ViewModel.IsDetailsVisible = true;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasHeroImage)));
    }

    public void Receive(HideDetailsMessage message) => HideDetails();

    public void Receive(LaunchUriMessage message) => _ = Launcher.LaunchUriAsync(message.Uri);

    public void Receive(HandleCommandResultMessage message) => HandleCommandResult(message.Result.Unsafe);

    private void HideDetails() => ViewModel.IsDetailsVisible = false;

    public void Receive(ClearSearchMessage message) => SearchBox.ClearSearch();

    private void GoBack()
    {
        HideDetails();

        // Note: That we restore the VM state below in RootFrame_Navigated call back after this occurs.
        // In the future, we may want to manage the back stack ourselves vs. relying on Frame
        // We could replace Frame with a ContentPresenter, but then have to manage transition animations ourselves.
        // However, then we have more fine-grained control on the back stack, managing the VM cache, and not
        // having that all be a black box, though then we wouldn't cache the XAML page itself, but sometimes that is a drawback.
        // However, we do a good job here, see ForwardStack.Clear below, and BackStack.Clear above about managing that.
        RootFrame.GoBack();

        // Don't store pages we're navigating away from in the Frame cache
        // TODO: In the future we probably want a short cache (3-5?) of recent VMs in case the user re-navigates
        // back to a recent page they visited (like the Pokedex) so we don't have to reload it from  scratch.
        // That'd be retrieved as we re-navigate in the PerformCommandMessage logic above
        RootFrame.ForwardStack.Clear();
        SearchBox.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
    }

    private void GoHome()
    {
        while (RootFrame.CanGoBack)
        {
            GoBack();
        }

        WeakReferenceMessenger.Default.Send<GoHomeMessage>();
    }

    private void BackButton_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e) => WeakReferenceMessenger.Default.Send<NavigateBackMessage>();

    private void RootFrame_Navigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        // This listens to the root frame to ensure that we also track the content's page VM as well that we passed as a parameter.
        // This is currently used for both forward and backward navigation.
        // As when we go back that we restore ourselves to the proper state within our VM
        if (e.Parameter is PageViewModel page)
        {
            // Note, this shortcuts and fights a bit with our LoadPageViewModel above, but we want to better fast display and incrementally load anyway
            // We just need to reconcile our loading systems a bit more in the future.
            ViewModel.CurrentPage = page;
        }
    }

    public bool HasHeroImage
    {
        get
        {
            var requestedTheme = ActualTheme;
            var iconInfo = ViewModel.Details?.HeroImage;
            var data = requestedTheme == Microsoft.UI.Xaml.ElementTheme.Dark ? iconInfo?.Dark : iconInfo?.Light;

            // We have an icon, AND EITHER:
            //      We have a string icon, OR
            //      we have a data blob
            var hasIcon = (data != null) &&
                    (!string.IsNullOrEmpty(data.Icon) ||
                    data.Data != null);
            return hasIcon;
        }
    }
}
