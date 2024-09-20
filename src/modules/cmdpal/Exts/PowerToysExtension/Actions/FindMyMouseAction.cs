// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using WindowsInput;
using WindowsInput.Native;

namespace PowerToysExtension.Actions;

internal sealed partial class FindMyMouseAction : InvokableCommand
{
    internal FindMyMouseAction()
    {
        this.Name = "Find My Mouse";
        this.Icon = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory.ToString(), "Assets\\FindMyMouse.png"));
    }

    public override ICommandResult Invoke()
    {
        try
        {
            var sim = new InputSimulator();

            // Simulate pressing the Ctrl key twice in short succession
            sim.Keyboard.KeyPress(VirtualKeyCode.LCONTROL); // First press
            Thread.Sleep(100); // Short delay
            sim.Keyboard.KeyPress(VirtualKeyCode.LCONTROL); // Second press

            return CommandResult.KeepOpen();
        }
        catch (Exception ex)
        {
            // Handle errors
            Debug.WriteLine($"Error sending keyboard shortcut: {ex.Message}");
            return CommandResult.KeepOpen();
        }
    }
}
