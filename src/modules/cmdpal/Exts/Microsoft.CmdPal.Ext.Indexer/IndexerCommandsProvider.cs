// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace Microsoft.CmdPal.Ext.Indexer;

public partial class IndexerActionsProvider : ICommandProvider
{
    public string DisplayName => $"Indexer Commands";

    public IconDataType Icon => new(string.Empty);

    private readonly IListItem[] _commands = [
        new ListItem(new IndexerPage()),
    ];

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public IListItem[] TopLevelCommands()
    {
        return _commands;
    }

    public void InitializeWithHost(IExtensionHost host)
    {
    }
}
