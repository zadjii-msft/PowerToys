// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.CmdPal.Common.Services;

namespace Microsoft.CmdPal.UI.ViewModels;

public sealed class TopLevelViewModel
{
    // TopLevelCommandItemWrapper is a ListItem, but it's in-memory for the app already.
    // We construct it either from data that we pulled from the cache, or from the
    // extension, but the data in it is all in our process now.
    private readonly TopLevelCommandItemWrapper _item;

    public IconInfoViewModel Icon { get; private set; }

    public string Title => _item.Title;

    public string Subtitle => _item.Subtitle;

    public List<CommandAlias> Aliases { get; private set; } = [];

    public bool HasAliases => Aliases.Count != 0;

    public TopLevelViewModel(TopLevelCommandItemWrapper item)
    {
        _item = item;
        Icon = new(item.Icon);
        Icon.InitializeProperties();
    }
}
