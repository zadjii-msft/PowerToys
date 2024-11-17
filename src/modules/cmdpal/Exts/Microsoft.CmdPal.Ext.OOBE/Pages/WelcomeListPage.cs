// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Windows.ApplicationModel.Email;

namespace Microsoft.CmdPal.Ext.OOBE;

internal sealed partial class WelcomeListPage : DynamicListPage
{
    private readonly ItemToComplete[] _listItems;

    public WelcomeListPage()
    {
        Icon = new(string.Empty);
        Name = "Getting Started";
        PlaceholderText = "Type here to filter the results";
        ShowDetails = true;

        var contextMenuListItem = new ItemToComplete(this)
        {
            Title = "Execute a Context Menu Command",
            Command = new NoOpCommand() { Name = "No Operation" },
            Details = new Details()
            {
                Title = "Context Menu Commands",
                Body = "Many items may have more than one command that you can invoke. You can find additional commands in the Context Menu [gif]. Access the context menu by typing `ctl + k`. Mark this item as complete by invoking the correct command in the Context Menu.",
            },
        };

        contextMenuListItem.MoreCommands = [new CommandContextItem(new ToggleCompletion(contextMenuListItem, this))];

        _listItems = [
            new ItemToComplete(this)
            {
                Title = "Filter items by typing",
                Details = new Details()
                {
                    Title = "Filtering Items",
                    Body = "You can filter items in the command palette by typing into the search box. To mark this item as completed, begin typing",
                },
            },
            new ItemToComplete(this)
            {
                Title = "Launch an Application From the Root View",
                Details = new Details()
                {
                    Title = "Launching Apps",
                    Body = "In the root view of the command palette, you can directly find and launch applications as shown in the below illustration. To mark this item as completed, launch an app directly from the command palette root view",
                },
            },
            new ItemToComplete(this)
            {
                Title = "Navigate Back to the Root View",
                Details = new Details()
                {
                    Title = "Navigate Throughout the Command Palette",
                    Body = "To navigate back to ,the root view, hit the \"ESC\" key or click on the bubble located by the search box.",
                },
            },
            new ItemToComplete(this)
            {
                Title = "Execute a Default Command",
                Details = new Details()
                {
                    Title = "Default Commands",
                    Body = "You can execute a default command by hitting Enter or double-clicking a highlighted list item. Execute the default command to mark this item as completed",
                },
            },
            contextMenuListItem,
            new ItemToComplete(this)
            {
                Title = "Install Your First Extension",
                Details = new Details()
                {
                    Title = "Installing Extensions",
                    Body = "The Windows Command Palette is designed to be helpful across a wide range of workflows. Extensions help you plug into the work that is most meaningful to you. You can install extensions directly from the Windows Command Palette [gif]. You can also install extensions from the Microsoft Store or Winget.",
                },
                MoreCommands = [
                    new CommandContextItem(new NoOpCommand()
                    {
                        Name = "Find Available Extensions",
                    })
                ],
            },
            new ItemToComplete(this)
            {
                Title = "Toggle Details View",
                Details = new Details()
                {
                    Title = "Details View",
                    Body = "The details view is an optional view that some Command Palette experiences utilize to provide additional information to users. You can always optionally toggle on/off the details view in the Context Menu. The Command Palette's default behavior is to show the details pane when it is available - you can configure this behavior in the Command Palette's settings.",
                },
            },
            new ItemToComplete(this)
            {
                Title = "Create an Alias to a Command",
                Details = new Details()
                {
                    Title = "Command Aliases",
                    Body = "You can create aliases for commands that appear in the palette's root view. An alias can help you quickly reference a specific command as shown in the illustration below [gif]. To complete this item, create a new command alias",
                },
            },
            new ItemToComplete(this)
            {
                Title = "Assign a Global Hotkey to a Command",
                Details = new Details()
                {
                    Title = "Global Hotkeys",
                    Body = "Global Hotkeys allow you to directly invoke a command with a keyboard shortcut regardless of the context you are in. [gif]",
                },
            }
        ];
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        if (_listItems == null)
        {
            return;
        }

        var filterItem = _listItems[0];

        if (!filterItem.CompletionStatus && newSearch.Length != oldSearch.Length)
        {
            filterItem.ToggleCompletion();
            RaiseItemsChanged(newSearch.Length);
        }
    }

    public sealed partial class ItemToComplete : ListItem
    {
        private readonly WelcomeListPage _welcomeListPage;

        private bool _isCompleted;

        public bool CompletionStatus => _isCompleted;

        public string GetSubtitle()
        {
            return CompletionStatus ? "Completed" : "Incomplete";
        }

        public IconDataType GetIcon()
        {
            return CompletionStatus ? new("\ue91B") : new("\ue8D2");
        }

        public ItemToComplete(WelcomeListPage welcomeListPage)
            : base(new NoOpCommand())
        {
            _isCompleted = false;
            Subtitle = GetSubtitle();
            Icon = GetIcon();
            Command = new ToggleCompletion(this, welcomeListPage);
            _welcomeListPage = welcomeListPage;
        }

        public void ToggleCompletion()
        {
            _isCompleted = !_isCompleted;
            this.Subtitle = GetSubtitle();
            this.Icon = GetIcon();
            _welcomeListPage.RaiseItemsChanged(0);
        }
    }

    public sealed partial class ToggleCompletion : InvokableCommand
    {
        private readonly ItemToComplete _listItem;
        private readonly WelcomeListPage _listPage;

        public string UpdateName()
        {
            return !_listItem.CompletionStatus ? "Mark Item as Completed" : "Mark Item as Incomplete";
        }

        public ToggleCompletion(ItemToComplete listItem, WelcomeListPage listPage)
        {
            _listItem = listItem;
            _listPage = listPage;
            Name = UpdateName();
        }

        public override CommandResult Invoke()
        {
            _listItem.ToggleCompletion();
            Name = UpdateName();
            _listPage.RaiseItemsChanged(0);
            return CommandResult.KeepOpen();
        }
    }

    public override IListItem[] GetItems()
    {
        return _listItems;
    }
}
