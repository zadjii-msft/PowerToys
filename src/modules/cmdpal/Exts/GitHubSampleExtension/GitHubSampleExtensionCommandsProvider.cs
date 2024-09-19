// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace GitHubSampleExtension;

public partial class GitHubSampleExtensionActionsProvider : ICommandProvider
{
    public string DisplayName => $"GitHub (tmp) extension for cmdpal Commands";

    public IconDataType Icon => new(string.Empty);

    private readonly IListItem[] _commands = [
        new ListItem(new GitHubSampleExtensionPage())
        {
            Title = "Search Issues",
        },
    ];

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
    public void Dispose() => throw new NotImplementedException();
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize

    public IListItem[] TopLevelCommands()
    {
        return _commands;
    }
}
