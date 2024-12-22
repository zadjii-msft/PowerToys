// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq;
using Microsoft.CmdPal.Ext.WindowWalker.Helpers;
using Microsoft.CmdPal.Ext.WindowWalker.Pages;
using Microsoft.CmdPal.Ext.WindowWalker.Properties;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Windows.Foundation;

namespace Microsoft.CmdPal.Ext.WindowWalker;

public partial class WindowWalkerCommandsProvider : CommandProvider
{
    private readonly CommandItem _windowWalkerPageItem;
    private readonly CommandItem _desktopsPageItem;

    internal static readonly VirtualDesktopHelper VirtualDesktopHelperInstance = new();

    public WindowWalkerCommandsProvider()
    {
        DisplayName = Resources.windowwalker_name;
        _windowWalkerPageItem = new CommandItem(new WindowWalkerListPage())
        {
            Title = Resources.window_walker_top_level_command_title,
            Subtitle = Resources.windowwalker_name,
            MoreCommands = [
                new CommandContextItem(new SettingsPage()),
            ],
        };
        _desktopsPageItem = new CommandItem(new VirtualDesktopsListPage())
        {
            Title = Resources.windowwalker_VirtualDesktopsCommandTitle,
            MoreCommands = [
                new CommandContextItem(new SettingsPage()),
            ],
        };
    }

    public override ICommandItem[] TopLevelCommands() => [_windowWalkerPageItem, _desktopsPageItem];
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "TODO")]
internal sealed partial class VirtualDesktopsListPage : ListPage
{
    public VirtualDesktopsListPage()
    {
        Icon = new("\uE7C4"); // TaskView
        Name = Resources.windowwalker_VirtualDesktopsCommandTitle;
        Id = "com.microsoft.cmdpal.virtual_desktops";
    }

    public override IListItem[] GetItems()
    {
        var instance = WindowWalkerCommandsProvider.VirtualDesktopHelperInstance;
        var desktops = instance.GetDesktopList();
        return desktops.Select(vd =>
        {
            var command = new SwitchToDesktopCommand(vd);
            var isCurrent = vd.IsVisible;
            return new ListItem(command)
            {
                Title = vd.Name ?? $"{Resources.windowwalker_Desktop} {vd.Number}",
                Subtitle = $"{vd.Number}",
                Tags = isCurrent ? [new Tag() { Text = Resources.windowwalker_CurrentDesktopTag }] : [],
            };
        }).ToArray();
    }
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This is sample code")]
internal sealed partial class SwitchToDesktopCommand : InvokableCommand
{
    private readonly VDesktop _desktop;

    public event TypedEventHandler<object, object?>? SwitchDesktopRequested;

    public SwitchToDesktopCommand(VDesktop desktop)
    {
        _desktop = desktop;
        Name = Resources.switch_to_command_title;
    }

    public override ICommandResult Invoke()
    {
        var instance = WindowWalkerCommandsProvider.VirtualDesktopHelperInstance;
        var hwnd = ExtensionHost.Host?.HostingHwnd ?? 0;
        if (hwnd != 0)
        {
            instance.MoveWindowToDesktop((nint)hwnd, _desktop.Id);
        }

        SwitchDesktopRequested?.Invoke(this, null);

        return CommandResult.Hide();
    }
}
