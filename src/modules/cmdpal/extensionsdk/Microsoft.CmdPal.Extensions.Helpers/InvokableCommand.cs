﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.CmdPal.Extensions.Helpers;

public abstract class InvokableCommand : Command, IInvokableCommand
{
    public abstract ICommandResult Invoke();

    public virtual ICommandResult Invoke(object? sender) => Invoke();
}
