// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace PowerToysExtension;

public partial class PowerToysExtensionActionsProvider : ICommandProvider
{
    public string DisplayName => $"PowerToys";

    public IconDataType Icon => new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory.ToString(), "Assets\\PowerToys.ico"));

    private readonly IListItem[] _commands = [
        new ListItem(new PowerToysExtensionPage()),
    ];

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
    public void Dispose() => throw new NotImplementedException();
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize

    public IListItem[] TopLevelCommands()
    {
        return _commands;
    }
}
