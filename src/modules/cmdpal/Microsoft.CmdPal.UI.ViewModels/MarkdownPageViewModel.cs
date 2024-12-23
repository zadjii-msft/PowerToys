// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.UI.ViewModels.Messages;
using Microsoft.CmdPal.UI.ViewModels.Models;

namespace Microsoft.CmdPal.UI.ViewModels;

public partial class MarkdownPageViewModel : PageViewModel
{
    private readonly ExtensionObject<IMarkdownPage> _model;

    public ObservableCollection<string> Bodies { get; set; } = [];

    public List<CommandContextItemViewModel> Commands { get; private set; } = [];

    public bool HasCommands => Commands.Count > 0;

    public DetailsViewModel? Details { get; private set; }

    [MemberNotNullWhen(true, nameof(Details))]
    public bool HasDetails => Details != null;

    // Remember - "observable" properties from the model (via PropChanged)
    // cannot be marked [ObservableProperty]
    public MarkdownPageViewModel(IMarkdownPage model, TaskScheduler scheduler, CommandPaletteHost host)
        : base(model, scheduler, host)
    {
        _model = new(model);
    }

    //// Run on background thread, from InitializeAsync or Model_ItemsChanged
    private void FetchContent()
    {
        try
        {
            var newItems = _model.Unsafe!.Bodies();

            Bodies.Clear();

            foreach (var item in newItems)
            {
                Bodies.Add(item);
            }

            UpdateProperty(nameof(HasDetails));
        }
        catch (Exception ex)
        {
            ShowException(ex);
            throw;
        }
    }

    public override void InitializeProperties()
    {
        base.InitializeProperties();

        var model = _model.Unsafe;
        if (model == null)
        {
            return; // throw?
        }

        Commands = model.Commands
            .Where(contextItem => contextItem is ICommandContextItem)
            .Select(contextItem => (contextItem as ICommandContextItem)!)
            .Select(contextItem => new CommandContextItemViewModel(contextItem, PageContext))
            .ToList();

        var extensionDetails = model.Details();
        if (extensionDetails != null)
        {
            Details = new(extensionDetails, PageContext);
            Details.InitializeProperties();
            UpdateDetails();
        }

        FetchContent();
    }

    protected override void FetchProperty(string propertyName)
    {
        base.FetchProperty(propertyName);

        var model = this._model.Unsafe;
        if (model == null)
        {
            return; // throw?
        }

        switch (propertyName)
        {
            // case nameof(Commands):
            //     this.ShowDetails = model.ShowDetails;
            //     break;
            case nameof(Details):
                var extensionDetails = model.Details();
                Details = extensionDetails != null ? new(extensionDetails, PageContext) : null;
                UpdateDetails();
                break;
        }

        UpdateProperty(propertyName);
    }

    private void UpdateDetails()
    {
        UpdateProperty(nameof(Details));
        UpdateProperty(nameof(HasDetails));

        Task.Factory.StartNew(
            () =>
            {
                if (HasDetails)
                {
                    WeakReferenceMessenger.Default.Send<ShowDetailsMessage>(new(Details));
                }
                else
                {
                    WeakReferenceMessenger.Default.Send<HideDetailsMessage>();
                }
            },
            CancellationToken.None,
            TaskCreationOptions.None,
            PageContext.Scheduler);
    }
}
