﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions.Toolkit;

namespace Microsoft.CmdPal.Ext.Apps.Programs;

internal sealed partial class AppListItem : ListItem
{
    private readonly AppItem _app;
    private static readonly Tag _appTag = new("App");
    private static readonly IconInfo _openPathIcon = new IconInfo("\ue838");

    public AppListItem(AppItem app)
        : base(new AppCommand(app))
    {
        _app = app;
        Title = app.Name;
        Subtitle = app.Subtitle;
        Tags = [_appTag];

        Details = new Details()
        {
            Title = this.Title,
            HeroImage = ((AppCommand)Command!).Icon ?? new IconInfo(string.Empty),
            Body = "### App",
        };

        if (string.IsNullOrEmpty(app.UserModelId))
        {
            // Win32 exe or other non UWP app
            MoreCommands = [
                new CommandContextItem(
                    new OpenPathCommand(app.DirPath)
                    {
                        Name = "Open location",
                        Icon = _openPathIcon,
                    })
            ];
        }
        else
        {
            // UWP app
            MoreCommands = [];
        }
    }
}
