﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.UI.ViewModels.Messages;
using Microsoft.CmdPal.UI.ViewModels.Models;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace Microsoft.CmdPal.UI.ViewModels;

public partial class CommandItemViewModel : ExtensionObjectViewModel, ICommandBarContext
{
    public ExtensionObject<ICommandItem> Model => _commandItemModel;

    private readonly ExtensionObject<ICommandItem> _commandItemModel = new(null);
    private CommandContextItemViewModel? _defaultCommandContextItem;

    protected bool IsFastInitialized { get; private set; }

    protected bool IsInitialized { get; private set; }

    protected bool IsSelectedInitialized { get; private set; }

    // These are properties that are "observable" from the extension object
    // itself, in the sense that they get raised by PropChanged events from the
    // extension. However, we don't want to actually make them
    // [ObservableProperty]s, because PropChanged comes in off the UI thread,
    // and ObservableProperty is not smart enough to raise the PropertyChanged
    // on the UI thread.
    public string Name => Command.Name;

    private string _itemTitle = string.Empty;

    public string Title => string.IsNullOrEmpty(_itemTitle) ? Name : _itemTitle;

    public string Subtitle { get; private set; } = string.Empty;

    private IconInfoViewModel _listItemIcon = new(null);

    public IconInfoViewModel Icon => _listItemIcon.IsSet ? _listItemIcon : Command.Icon;

    public CommandViewModel Command { get; private set; }

    public List<CommandContextItemViewModel> MoreCommands { get; private set; } = [];

    IEnumerable<CommandContextItemViewModel> ICommandBarContext.MoreCommands => MoreCommands;

    public bool HasMoreCommands => MoreCommands.Count > 0;

    public string SecondaryCommandName => SecondaryCommand?.Name ?? string.Empty;

    public CommandItemViewModel? PrimaryCommand => this;

    public CommandItemViewModel? SecondaryCommand => HasMoreCommands ? MoreCommands[0] : null;

    public bool ShouldBeVisible => !string.IsNullOrEmpty(Name);

    public List<CommandContextItemViewModel> AllCommands
    {
        get
        {
            List<CommandContextItemViewModel> l = _defaultCommandContextItem == null ?
                new() :
                [_defaultCommandContextItem];

            l.AddRange(MoreCommands);
            return l;
        }
    }

    public CommandItemViewModel(ExtensionObject<ICommandItem> item, IPageContext errorContext)
        : base(errorContext)
    {
        _commandItemModel = item;
        Command = new(null, errorContext);
    }

    public void FastInitializeProperties()
    {
        if (IsFastInitialized)
        {
            return;
        }

        var model = _commandItemModel.Unsafe;
        if (model == null)
        {
            return;
        }

        Command = new(model.Command, PageContext);
        Command.FastInitializeProperties();

        _itemTitle = model.Title;
        Subtitle = model.Subtitle;

        IsFastInitialized = true;
    }

    //// Called from ListViewModel on background thread started in ListPage.xaml.cs
    public override void InitializeProperties()
    {
        if (IsInitialized)
        {
            return;
        }

        if (!IsFastInitialized)
        {
            FastInitializeProperties();
        }

        var model = _commandItemModel.Unsafe;
        if (model == null)
        {
            return;
        }

        // Command = new(model.Command, PageContext);
        Command.InitializeProperties();

        // _itemTitle = model.Title;
        // Subtitle = model.Subtitle;
        var listIcon = model.Icon;
        if (listIcon != null)
        {
            _listItemIcon = new(listIcon);
            _listItemIcon.InitializeProperties();
        }

        // TODO: Do these need to go into FastInit?
        model.PropChanged += Model_PropChanged;
        Command.PropertyChanged += Command_PropertyChanged;

        UpdateProperty(nameof(Name));
        UpdateProperty(nameof(Title));
        UpdateProperty(nameof(Subtitle));
        UpdateProperty(nameof(Icon));
        UpdateProperty(nameof(IsInitialized));

        IsInitialized = true;
    }

    public void SlowInitializeProperties()
    {
        if (IsSelectedInitialized)
        {
            return;
        }

        if (!IsInitialized)
        {
            InitializeProperties();
        }

        var model = _commandItemModel.Unsafe;
        if (model == null)
        {
            return;
        }

        var more = model.MoreCommands;
        if (more != null)
        {
            MoreCommands = more
                .Where(contextItem => contextItem is ICommandContextItem)
                .Select(contextItem => (contextItem as ICommandContextItem)!)
                .Select(contextItem => new CommandContextItemViewModel(contextItem, PageContext))
                .ToList();
        }

        // Here, we're already theoretically in the async context, so we can
        // use Initialize straight up
        MoreCommands.ForEach(contextItem =>
        {
            contextItem.InitializeProperties();
        });

        _defaultCommandContextItem = new(new CommandContextItem(model.Command!), PageContext)
        {
            _itemTitle = Name,
            Subtitle = Subtitle,

            // _listItemIcon = _listItemIcon,
            // Command = new(model.Command, PageContext),
            Command = Command,

            // TODO this probably should just be a CommandContextItemViewModel(CommandItemViewModel) ctor, or a copy ctor or whatever
        };

        // Only set the icon on the context item for us if our command didn't
        // have its own icon
        if (!Command.HasIcon)
        {
            _defaultCommandContextItem._listItemIcon = _listItemIcon;
        }

        IsSelectedInitialized = true;
        UpdateProperty(nameof(MoreCommands));
        UpdateProperty(nameof(AllCommands));
        UpdateProperty(nameof(IsSelectedInitialized));
    }

    public bool SafeFastInit()
    {
        try
        {
            FastInitializeProperties();
            return true;
        }
        catch (Exception)
        {
            Command = new(null, PageContext);
            _itemTitle = string.Empty;
            Subtitle = string.Empty;
            IsInitialized = true;
            IsSelectedInitialized = true;
        }

        return false;
    }

    public bool SafeSlowInit()
    {
        try
        {
            SlowInitializeProperties();
            return true;
        }
        catch (Exception)
        {
            IsSelectedInitialized = true;
        }

        return false;
    }

    public bool SafeInitializeProperties()
    {
        try
        {
            InitializeProperties();
            return true;
        }
        catch (Exception)
        {
            Command = new(null, PageContext);
            _itemTitle = string.Empty;
            Subtitle = string.Empty;
            IsInitialized = true;
            IsSelectedInitialized = true;
        }

        return false;
    }

    private void Model_PropChanged(object sender, IPropChangedEventArgs args)
    {
        try
        {
            FetchProperty(args.PropertyName);
        }
        catch (Exception ex)
        {
            PageContext.ShowException(ex, _commandItemModel?.Unsafe?.Title);
        }
    }

    protected virtual void FetchProperty(string propertyName)
    {
        var model = this._commandItemModel.Unsafe;
        if (model == null)
        {
            return; // throw?
        }

        switch (propertyName)
        {
            case nameof(Command):
                if (Command != null)
                {
                    Command.PropertyChanged -= Command_PropertyChanged;
                }

                Command = new(model.Command, PageContext);
                Command.InitializeProperties();
                UpdateProperty(nameof(Name));
                UpdateProperty(nameof(Title));
                UpdateProperty(nameof(Icon));
                break;

            case nameof(Title):
                _itemTitle = model.Title;
                break;

            case nameof(Subtitle):
                this.Subtitle = model.Subtitle;
                break;

            case nameof(Icon):
                _listItemIcon = new(model.Icon);
                _listItemIcon.InitializeProperties();
                break;

            case nameof(model.MoreCommands):
                var more = model.MoreCommands;
                if (more != null)
                {
                    var newContextMenu = more
                        .Where(contextItem => contextItem is ICommandContextItem)
                        .Select(contextItem => (contextItem as ICommandContextItem)!)
                        .Select(contextItem => new CommandContextItemViewModel(contextItem, PageContext))
                        .ToList();
                    lock (MoreCommands)
                    {
                        ListHelpers.InPlaceUpdateList(MoreCommands, newContextMenu);
                    }

                    MoreCommands.ForEach(contextItem =>
                    {
                        contextItem.InitializeProperties();
                    });
                }
                else
                {
                    MoreCommands.Clear();
                }

                UpdateProperty(nameof(SecondaryCommand));
                UpdateProperty(nameof(SecondaryCommandName));
                UpdateProperty(nameof(HasMoreCommands));

                break;
        }

        UpdateProperty(propertyName);
    }

    private void Command_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        var propertyName = e.PropertyName;
        switch (propertyName)
        {
            case nameof(Command.Name):
                UpdateProperty(nameof(Title));
                UpdateProperty(nameof(Name));
                break;
            case nameof(Command.Icon):
                UpdateProperty(nameof(Icon));
                break;
        }
    }
}
