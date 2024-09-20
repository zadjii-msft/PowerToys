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
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using WindowsInput;
using WindowsInput.Native;

namespace PowerToysExtension.Actions;

internal sealed partial class MouseCrosshairsAction : InvokableCommand
{
    internal MouseCrosshairsAction()
    {
        this.Name = "Mouse Pointer Crosshairs";
        this.Icon = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory.ToString(), "Assets\\MouseCrosshairs.png"));
    }

    public override ICommandResult Invoke()
    {
        try
        {
            var sim = new InputSimulator();

            // Simulate holding down Left Windows key and Left Alt key, then pressing 'P'
            sim.Keyboard.ModifiedKeyStroke(new[] { VirtualKeyCode.LWIN, VirtualKeyCode.LMENU }, VirtualKeyCode.VK_P);

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
