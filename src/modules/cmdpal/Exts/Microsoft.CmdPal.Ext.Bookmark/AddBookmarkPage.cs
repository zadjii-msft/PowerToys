// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Ext.Bookmarks.Properties;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.Foundation;

namespace Microsoft.CmdPal.Ext.Bookmarks;

internal sealed partial class AddBookmarkPage : ContentPage
{
    private readonly AddBookmarkForm _addBookmark;

    internal event TypedEventHandler<object, object?>? AddedCommand
    {
        add => _addBookmark.AddedCommand += value;
        remove => _addBookmark.AddedCommand -= value;
    }

    public override IContent[] GetContent() => [_addBookmark];

    public AddBookmarkPage(string name = "", string url = "")
    {
        Icon = new IconInfo("\ued0e");
        var isAdd = string.IsNullOrEmpty(name) && string.IsNullOrEmpty(url);
        Title = isAdd ? Resources.bookmarks_add_title : Resources.bookmarks_edit_name;
        Name = isAdd ? Resources.bookmarks_add_name : Resources.bookmarks_edit_name;
        _addBookmark = new(name, url);
    }
}
