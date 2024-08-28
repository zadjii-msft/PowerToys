// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.CommandPalette.Extensions;
using System.ComponentModel;
using Microsoft.UI.Dispatching;
using CmdPal.Models;

namespace DeveloperCommandPalette;

public sealed class ListItemViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly DispatcherQueue DispatcherQueue;
    internal ExtensionObject<IListItem> ListItem { get; init; }
    internal string Title { get; private set; }
    internal string Subtitle { get; private set; }
    internal string Icon { get; private set; }

    internal Lazy<DetailsViewModel?> _Details;
    internal DetailsViewModel? Details => _Details.Value;
    internal IFallbackHandler? FallbackHandler => this.ListItem.Safe?.FallbackHandler;

    public event PropertyChangedEventHandler? PropertyChanged;

    internal ICommand? DefaultAction => ListItem.Safe?.Command;
    internal bool CanInvoke => DefaultAction != null && DefaultAction is IInvokableCommand or IPage;
    internal IconElement IcoElement => Microsoft.Terminal.UI.IconPathConverter.IconMUX(Icon);

    private IEnumerable<ICommandContextItem> contextActions
    {
        get {
            var safe = ListItem.Safe;
            if (safe == null) return [];
            return safe.MoreCommands == null ?
                [] :
                safe.MoreCommands.Where(i => i is ICommandContextItem).Select(i => (ICommandContextItem)i);
        } 
    }
    internal bool HasMoreCommands => contextActions.Any();

    internal TagViewModel[] Tags = [];
    internal bool HasTags => Tags.Length > 0;

    internal IList<ContextItemViewModel> ContextActions
    {
        get
        {
            var safe = ListItem.Safe;
            if (safe == null) return [];

            var l = contextActions.Select(a => new ContextItemViewModel(a)).ToList();
            var def = DefaultAction;
            if (def!=null) l.Insert(0, new(def));
            return l;
        }
    }

    public ListItemViewModel(IListItem model)
    {
        model.PropChanged += ListItem_PropertyChanged;
        this.ListItem = new(model);
        this.Title = model.Title;
        this.Subtitle = model.Subtitle;
        this.Icon = model.Command.Icon.Icon;
        if (model.Tags != null)
        {
            this.Tags = model.Tags.Select(t => new TagViewModel(t)).ToArray();
        }

        this._Details = new(() => {
            // TODO! even this pattern of "get a safe ref, then keep using it" isn't safe
            var safe = this.ListItem.Safe;
            if (safe != null)
            {
                return safe.Details != null ? new(safe.Details) : null;
            }
            else return null;
        });

        this.DispatcherQueue = DispatcherQueue.GetForCurrentThread();
    }

    private void ListItem_PropertyChanged(object sender, Microsoft.Windows.CommandPalette.Extensions.PropChangedEventArgs args)
    {
        switch (args.PropertyName)
        {
            case "Name":
            case nameof(Title):
                {
                    this.Title = ListItem.Safe?.Title ?? this.Title;
                }
                break;
            case nameof(Subtitle):
                {
                    this.Subtitle = ListItem.Safe?.Subtitle ?? this.Subtitle;
                }
                break;
            case "MoreCommands":
                {
                    BubbleXamlPropertyChanged(nameof(HasMoreCommands));
                    BubbleXamlPropertyChanged(nameof(ContextActions));
                }
                break;
            case nameof(Icon):
                {
                    this.Icon = ListItem.Safe?.Command.Icon.Icon ?? this.Icon;
                    BubbleXamlPropertyChanged(nameof(IcoElement));
                }
                break;
        }
        BubbleXamlPropertyChanged(args.PropertyName);

    }

    private void BubbleXamlPropertyChanged(string propertyName)
    {
        if (this.DispatcherQueue == null)
        {
            // this is highly unusual
            return;
        }
        this.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
        {
            this.PropertyChanged?.Invoke(this, new(propertyName));
        });
    }

    public void Dispose()
    {
        var safe = this.ListItem.Safe;
        if (safe != null)
        {
            safe.PropChanged -= ListItem_PropertyChanged;
        }
    }
}
