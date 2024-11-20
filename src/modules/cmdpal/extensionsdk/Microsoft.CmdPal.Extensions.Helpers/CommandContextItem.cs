﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.CmdPal.Extensions.Helpers;

public partial class CommandContextItem : CommandItem, ICommandContextItem
{
    public bool IsCritical { get; set; }

    public CommandContextItem(ICommand command)
        : base(command)
    {
    }
}
