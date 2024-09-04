﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using Microsoft.Windows.CommandPalette.Extensions.Helpers;

namespace ProcessMonitorExtension;

internal sealed class SwitchToProcess : InvokableCommand
{
    [DllImport("user32.dll")]
    public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

    private readonly ProcessItem process;

    public SwitchToProcess(ProcessItem process)
    {
        this.process = process;
        this.Icon = new(process.ExePath == string.Empty ? "\uE7B8" : process.ExePath);
        this.Name = "Switch to";
    }

    public override ActionResult Invoke()
    {
        SwitchToThisWindow(process.Process.MainWindowHandle, true);
        return ActionResult.KeepOpen();
    }
}