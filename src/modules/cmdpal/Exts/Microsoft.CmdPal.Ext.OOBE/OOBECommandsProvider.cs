// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.CmdPal.Ext.OOBE.TopLevelListItems;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace Microsoft.CmdPal.Ext.OOBE;

public partial class OOBECommandsProvider : CommandProvider
{
    private readonly WelcomeListItem welcomeCommand = new();

    public OOBECommandsProvider()
    {
        DisplayName = $"Welcome to Windows Command Palette";
    }

    public override IListItem[] TopLevelCommands()
    {
        return [welcomeCommand];
    }
}
