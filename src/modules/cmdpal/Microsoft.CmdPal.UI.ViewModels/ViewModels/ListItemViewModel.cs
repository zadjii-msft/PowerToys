// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Models;

namespace Microsoft.CmdPal.UI.ViewModels;

public partial class ListItemViewModel : ObservableObject
{
    private readonly ExtensionObject<IListItem> _listItemModel;

    public string Title => _listItemModel.Unsafe.Title;

    public string Subtitle => _listItemModel.Unsafe.Subtitle;

    /// <summary>
    /// Gets the path for the icon to load in the View layer. TODO: Converter/Cache
    /// </summary>
    public string IconUri => _listItemModel.Unsafe.Icon.Icon;

    public ITag[] Tags => _listItemModel.Unsafe.Tags;

    public ICommand Command => _listItemModel.Unsafe.Command;

    public bool HasTags => Tags.Length > 0;

    public bool HasMoreCommands => _listItemModel.Unsafe.MoreCommands.Length > 0;

    // TODO this may also be separators, but good enough for now.
    public IEnumerable<ICommandContextItem> MoreCommands => _listItemModel.Unsafe.MoreCommands
        .Where(item => item is ICommandContextItem)
        .Select(i => (i as ICommandContextItem)!);

    public ListItemViewModel(IListItem model)
    {
        _listItemModel = new(model);
        _listItemModel.Unsafe.PropChanged += Model_PropChanged;
    }

    private void Model_PropChanged(object sender, PropChangedEventArgs args) => OnPropertyChanged(args.PropertyName);
}
