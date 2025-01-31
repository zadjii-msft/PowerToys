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
using Windows.Foundation;

namespace MicrosoftLearnExtension;

public partial class MicrosoftLearnExtensionActionsProvider : CommandProvider
{
    public MicrosoftLearnExtensionActionsProvider()
    {
        DisplayName = $"Microsoft Learn Doc Search Extension for cmdpal Commands";
    }

    private readonly ICommandItem[] _actions = [
        new CommandItem(new MicrosoftLearnExtensionPage()),
    ];

    public override ICommandItem[] TopLevelCommands() => _actions;
}
