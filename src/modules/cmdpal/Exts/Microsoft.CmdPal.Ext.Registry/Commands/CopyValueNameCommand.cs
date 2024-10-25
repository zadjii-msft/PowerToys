// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CmdPal.Ext.Registry.Classes;
using Microsoft.CmdPal.Ext.Registry.Helpers;
using Microsoft.CmdPal.Ext.Registry.Properties;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI;

namespace Microsoft.CmdPal.Ext.Registry.Commands;

internal sealed partial class CopyValueNameCommand : InvokableCommand
{
    private readonly RegistryEntry _entry;

    internal CopyValueNameCommand(RegistryEntry entry)
    {
        Name = Resources.CopyValueName;
        Icon = new("\xE8C8");
        _entry = entry;
    }

    private static bool TryToCopyToClipBoard(in string text)
    {
        // TODO: Have this actually use the clipboard helper
        return true;
    }

    public override CommandResult Invoke()
    {
        TryToCopyToClipBoard(_entry.GetValueNameWithKey());

        return CommandResult.KeepOpen();
    }
}
