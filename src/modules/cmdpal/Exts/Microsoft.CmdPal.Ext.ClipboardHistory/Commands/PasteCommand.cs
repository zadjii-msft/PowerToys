// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CmdPal.Ext.ClipboardHistory.Helpers;
using Microsoft.CmdPal.Ext.ClipboardHistory.Models;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Windows.ApplicationModel.DataTransfer;
using Windows.Networking.NetworkOperators;
using Windows.System;
using WinRT.Interop;

namespace Microsoft.CmdPal.Ext.ClipboardHistory.Commands;

internal sealed partial class PasteCommand : InvokableCommand
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private readonly ClipboardItem _clipboardItem;

    private const int HIDE = 0;
    private const int SHOW = 5;

    internal PasteCommand(ClipboardItem clipboardItem)
    {
        _clipboardItem = clipboardItem;
        Name = "Paste";
        Icon = new("\xE8C8"); // Copy icon
    }

    private static void SendSingleKeyboardInput(short keyCode, uint keyStatus)
    {
        var ignoreKeyEventFlag = (UIntPtr)0x5555;

        NativeMethods.INPUT inputShift = new NativeMethods.INPUT
        {
            type = NativeMethods.INPUTTYPE.INPUT_KEYBOARD,
            data = new NativeMethods.InputUnion
            {
                ki = new NativeMethods.KEYBDINPUT
                {
                    wVk = keyCode,
                    dwFlags = keyStatus,

                    // Any keyevent with the extraInfo set to this value will be ignored by the keyboard hook and sent to the system instead.
                    dwExtraInfo = ignoreKeyEventFlag,
                },
            },
        };

        NativeMethods.INPUT[] inputs = new NativeMethods.INPUT[] { inputShift };
        _ = NativeMethods.SendInput(1, inputs, NativeMethods.INPUT.Size);
    }

    internal static void SendPasteKeyCombination()
    {
        SendSingleKeyboardInput((short)VirtualKey.LeftControl, (uint)NativeMethods.KeyEventF.KeyUp);
        SendSingleKeyboardInput((short)VirtualKey.RightControl, (uint)NativeMethods.KeyEventF.KeyUp);
        SendSingleKeyboardInput((short)VirtualKey.LeftWindows, (uint)NativeMethods.KeyEventF.KeyUp);
        SendSingleKeyboardInput((short)VirtualKey.RightWindows, (uint)NativeMethods.KeyEventF.KeyUp);
        SendSingleKeyboardInput((short)VirtualKey.LeftShift, (uint)NativeMethods.KeyEventF.KeyUp);
        SendSingleKeyboardInput((short)VirtualKey.RightShift, (uint)NativeMethods.KeyEventF.KeyUp);
        SendSingleKeyboardInput((short)VirtualKey.LeftMenu, (uint)NativeMethods.KeyEventF.KeyUp);
        SendSingleKeyboardInput((short)VirtualKey.RightMenu, (uint)NativeMethods.KeyEventF.KeyUp);

        // Send Ctrl + V
        SendSingleKeyboardInput((short)VirtualKey.Control, (uint)NativeMethods.KeyEventF.KeyDown);
        SendSingleKeyboardInput((short)VirtualKey.V, (uint)NativeMethods.KeyEventF.KeyDown);
        SendSingleKeyboardInput((short)VirtualKey.V, (uint)NativeMethods.KeyEventF.KeyUp);
        SendSingleKeyboardInput((short)VirtualKey.Control, (uint)NativeMethods.KeyEventF.KeyUp);
    }

    private void HideWindow()
    {
        var hostHwnd = ExtensionHost.Host.HostingHwnd;

        ShowWindow(new IntPtr((long)hostHwnd), HIDE);
    }

    private void ShowWindow()
    {
        var hostHwnd = ExtensionHost.Host.HostingHwnd;

        ShowWindow(new IntPtr((long)hostHwnd), SHOW);
    }

    public override CommandResult Invoke()
    {
        HideWindow();
        SendPasteKeyCombination();
        ShowWindow();

        return CommandResult.Dismiss();
    }
}
