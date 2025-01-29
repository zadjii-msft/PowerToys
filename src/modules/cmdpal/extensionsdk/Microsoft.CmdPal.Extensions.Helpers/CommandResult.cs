// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.CmdPal.Extensions.Helpers;

public partial class CommandResult : ICommandResult
{
    private CommandResultKind _kind = CommandResultKind.Dismiss;

    // TODO: is Args needed?
    public ICommandResultArgs? Args { get; private set; }

    public CommandResultKind Kind => _kind;

    public static CommandResult Dismiss()
    {
        return new CommandResult()
        {
            _kind = CommandResultKind.Dismiss,
        };
    }

    public static CommandResult GoHome()
    {
        return new CommandResult()
        {
            _kind = CommandResultKind.GoHome,
            Args = null,
        };
    }

    public static CommandResult GoBack()
    {
        return new CommandResult()
        {
            _kind = CommandResultKind.GoBack,
            Args = null,
        };
    }

    public static CommandResult Hide()
    {
        return new CommandResult()
        {
            _kind = CommandResultKind.Hide,
            Args = null,
        };
    }

    public static CommandResult KeepOpen()
    {
        return new CommandResult()
        {
            _kind = CommandResultKind.KeepOpen,
            Args = null,
        };
    }
}
