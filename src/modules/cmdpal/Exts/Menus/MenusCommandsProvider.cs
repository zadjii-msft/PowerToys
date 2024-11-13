// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Microsoft.Win32.SafeHandles;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Menus;

public partial class MenusActionsProvider : CommandProvider
{
    public MenusActionsProvider()
    {
        DisplayName = $"Menus from the open windows Commands";
    }

    private readonly IListItem[] _commands = [
        new ListItem(new AllWindowsPage()),
    ];

    public override IListItem[] TopLevelCommands()
    {
        return _commands;
    }
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This is sample code")]
internal sealed class SafeMenu : SafeHandleZeroOrMinusOneIsInvalid
{
    public SafeMenu()
        : base(true)
    {
    }

    public void SetHandle(HMENU h)
    {
        this.handle = h;
    }

    protected override bool ReleaseHandle()
    {
        return true;
    }
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This is sample code")]
internal sealed class WindowData
{
    private readonly HWND handle;

    private readonly string title = string.Empty;

    public string Title => title;

    internal WindowData(HWND hWnd)
    {
        handle = hWnd;
        var textLen = PInvoke.GetWindowTextLength(handle);
        if (textLen == 0)
        {
            return;
        }

        var bufferSize = textLen + 1;
        unsafe
        {
            fixed (char* windowNameChars = new char[bufferSize])
            {
                if (PInvoke.GetWindowText(handle, windowNameChars, bufferSize) == 0)
                {
                    var errorCode = Marshal.GetLastWin32Error();
                    if (errorCode != 0)
                    {
                        throw new Win32Exception(errorCode);
                    }
                }

                title = new string(windowNameChars);
            }
        }
    }

    public List<string> GetMenuItems()
    {
        var s = new SafeMenu();
        var hMenu = PInvoke.GetMenu(handle);
        s.SetHandle(hMenu);
        List<string> results = new();
        var menuItemCount = PInvoke.GetMenuItemCount(hMenu);
        for (var i = 0; i < menuItemCount; i++)
        {
            var mii = default(MENUITEMINFOW);
            mii.cbSize = (uint)Marshal.SizeOf<MENUITEMINFOW>();
            mii.fMask = MENU_ITEM_MASK.MIIM_STRING | MENU_ITEM_MASK.MIIM_ID | MENU_ITEM_MASK.MIIM_SUBMENU;
            mii.cch = 256;

            unsafe
            {
                fixed (char* menuTextBuffer = new char[mii.cch])
                {
                    mii.dwTypeData = new PWSTR(menuTextBuffer); // Allocate memory for string

                    if (PInvoke.GetMenuItemInfo(s, (uint)i, true, ref mii))
                    {
                        var itemText = mii.dwTypeData.ToString();

                        // Leaf item
                        if (mii.hSubMenu == IntPtr.Zero)
                        {
                            // Console.WriteLine($"- Leaf Item: {itemText}");
                            // TriggerMenuItem(hWnd, mii.wID);
                            results.Add(itemText);
                        }
                        else
                        {
                            // Recursively list submenu items
                            // ListMenuItems(hWnd, mii.hSubMenu);
                        }
                    }
                }
            }
        }

        return results;
    }
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This is sample code")]
internal sealed partial class WindowMenusPage : ListPage
{
    private readonly WindowData _window;

    public WindowMenusPage(WindowData window)
    {
        _window = window;
        Icon = new(string.Empty);
        Name = window.Title;
        ShowDetails = false;
    }

    public override IListItem[] GetItems()
    {
        return _window.GetMenuItems().Select(caption => new ListItem(new NoOpCommand()) { Title = caption }).ToArray();
    }
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This is sample code")]
internal sealed partial class AllWindowsPage : ListPage
{
    private readonly List<WindowData> windows = new();

    public AllWindowsPage()
    {
        Icon = new(string.Empty);
        Name = "Open Windows";
        ShowDetails = false;
    }

    public override IListItem[] GetItems()
    {
        PInvoke.EnumWindows(EnumWindowsCallback, IntPtr.Zero);

        return windows
            .Where(w => !string.IsNullOrEmpty(w.Title))
            .Select(w => new ListItem(new WindowMenusPage(w))
            {
                Title = w.Title,
            })
            .ToArray();
    }

    private BOOL EnumWindowsCallback(HWND hWnd, LPARAM lParam)
    {
        // Only consider top-level visible windows with menus
        if (/*PInvoke.IsWindowVisible(hWnd) &&*/ PInvoke.GetMenu(hWnd) != IntPtr.Zero)
        {
            try
            {
                windows.Add(new(hWnd));
            }
            catch (Exception)
            {
            }

            return true;
        }

        return true; // Continue enumeration
    }
}
