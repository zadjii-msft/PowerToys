// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Microsoft.CmdPal.UI.ViewModels.Models;

namespace Microsoft.CmdPal.UI.ViewModels;

public partial class TopLevelCommandWrapper : ListItem
{
    public ExtensionObject<ICommandItem> Model { get; }

    public TopLevelCommandWrapper(ExtensionObject<ICommandItem> commandItem)
        : base(commandItem.Unsafe?.Command ?? new NoOpCommand())
    {
        // TODO! In reality we should do an async fetch
        Model = commandItem;
        try
        {
            var model = Model.Unsafe;
            if (model == null)
            {
                return;
            }

            // Name = model.Command?.Name ?? string.Empty;
            Title = model.Title;
            Subtitle = model.Subtitle;
            Icon = new(model.Icon.Icon);
            MoreCommands = model.MoreCommands;

            // .Where(contextItem => contextItem is ICommandContextItem)
            // .Select(contextItem => (contextItem as ICommandContextItem)!)
            // .Select(contextItem => new CommandContextItemViewModel(contextItem))
            // .ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }
}
