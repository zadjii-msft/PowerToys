﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.UI.ViewModels.Models;

namespace Microsoft.CmdPal.UI.ViewModels;

public partial class CommandItemViewModel : ObservableObject
{
    private readonly ExtensionObject<ICommandItem> _commandItemModel = new(null);

    // private bool _initialized;
    public string Name { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string Subtitle { get; private set; } = string.Empty;

    public string IconUri { get; private set; } = string.Empty;

    public ExtensionObject<ICommand> Command { get; } = new(null);

    public List<CommandContextItemViewModel> MoreCommands { get; private set; } = [];

    public bool HasMoreCommands => MoreCommands.Count > 0;

    public CommandItemViewModel(ExtensionObject<ICommandItem> item)
    {
        _commandItemModel = item;
    }

    public async virtual Task InitializePropertiesAsync()
    {
        var t = new Task(() =>
        {
            try
            {
                Initialize();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        });
        t.Start();
        await t;
    }

    protected virtual void Initialize()
    {
        var model = _commandItemModel.Unsafe;
        if (model == null)
        {
            return;
        }

        Name = model.Command?.Name ?? string.Empty;
        Title = model.Title;
        Subtitle = model.Subtitle;
        IconUri = model.Icon.Icon;
        MoreCommands = model.MoreCommands
            .Where(contextItem => contextItem is ICommandContextItem)
            .Select(contextItem => (contextItem as ICommandContextItem)!)
            .Select(contextItem => new CommandContextItemViewModel(contextItem)) // TODO ContextItemViewModels
            .ToList();

        model.PropChanged += Model_PropChanged;

        // _initialized = true;
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
        var model = this._commandItemModel.Unsafe;
        if (model == null)
        {
            return; // throw?
        }

        switch (propertyName)
        {
            case nameof(Name):
                this.Name = model.Command?.Name ?? string.Empty;
                break;
            case nameof(Title):
                this.Title = model.Title;
                break;
            case nameof(Subtitle):
                this.Subtitle = model.Subtitle;
                break;

                // TODO! Icon
                // TODO! MoreCommands array, which needs to also raise HasMoreCommands
        }
    }
}
