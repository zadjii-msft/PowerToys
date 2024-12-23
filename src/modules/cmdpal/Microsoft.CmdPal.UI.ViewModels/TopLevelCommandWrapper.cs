// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.UI.ViewModels.Models;
using Windows.Foundation;

namespace Microsoft.CmdPal.UI.ViewModels;

public partial class TopLevelCommandWrapper : ICommand
{
    private readonly ExtensionObject<ICommand> _command;

    public event TypedEventHandler<object, PropChangedEventArgs>? PropChanged;

    public string Name { get; private set; } = string.Empty;

    public string Id { get; private set; } = string.Empty;

    public IconDataType Icon { get; private set; } = new(string.Empty);

    public ICommand Command => _command.Unsafe!;

    public CommandPaletteHost? ExtensionHost { get; set; }

    public TopLevelCommandWrapper(ICommand command)
    {
        _command = new(command);
    }

    public void UnsafeInitializeProperties()
    {
        var model = _command.Unsafe!;

        Name = model.Name;
        Id = model.Id;
        Icon = model.Icon;

        model.PropChanged += this.PropChanged;
    }
}
