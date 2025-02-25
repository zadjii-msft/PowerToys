// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.CmdPal.UI.ViewModels.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.CmdPal.UI.ViewModels;

public sealed partial class TopLevelViewModel : ObservableObject
{
    private readonly SettingsModel _settings;
    private readonly IServiceProvider _serviceProvider;

    // TopLevelCommandItemWrapper is a ListItem, but it's in-memory for the app already.
    // We construct it either from data that we pulled from the cache, or from the
    // extension, but the data in it is all in our process now.
    private readonly TopLevelCommandItemWrapper _item;

    public IconInfoViewModel Icon { get; private set; }

    public string Title => _item.Title;

    public string Subtitle => _item.Subtitle;

    public HotkeySettings? Hotkey
    {
        get => _item.Hotkey;
        set
        {
            _serviceProvider.GetService<HotkeyManager>()!.UpdateHotkey(_item.Id, value);
            _item.Hotkey = value;
            Save();
        }
    }

    public string AliasText
    {
        get => field;
        set
        {
            field = value;
            UpdateAlias();
        }
    }

    public bool IsDirectAlias
    {
        get => field;
        set
        {
            field = value;
            UpdateAlias();
        }
    }

    public TopLevelViewModel(TopLevelCommandItemWrapper item, SettingsModel settings, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _settings = settings;

        _item = item;
        Icon = new(item.Icon ?? item.Command?.Icon);
        Icon.InitializeProperties();

        var aliases = _serviceProvider.GetService<AliasManager>()!;
        AliasText = _item.Alias ?? string.Empty;
    }

    private void Save() => SettingsModel.SaveSettings(_settings);

    private void UpdateAlias()
    {
        var newAliasString = string.IsNullOrWhiteSpace(AliasText) ?
            string.Empty :
            AliasText + (IsDirectAlias ? string.Empty : " ");

        _item.UpdateAlias(newAliasString);
        Save();
    }
}
