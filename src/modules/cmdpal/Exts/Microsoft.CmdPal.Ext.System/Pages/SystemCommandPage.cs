// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Ext.System.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace Microsoft.CmdPal.Ext.System.Pages;

internal sealed partial class SystemCommandPage : ListPage
{
    public SystemCommandPage()
    {
        Title = "SystemCommandPage";
        Name = "Open";
    }

    public override IListItem[] GetItems() => Commands.GetAllCommands().ToArray();
}
