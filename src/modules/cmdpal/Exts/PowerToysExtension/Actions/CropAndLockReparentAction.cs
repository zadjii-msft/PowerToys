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

internal sealed partial class CropAndLockReparentAction : InvokableCommand
{
    internal CropAndLockReparentAction()
    {
        this.Name = "Crop And Lock (Reparent)";
        this.Icon = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory.ToString(), "Assets\\CropAndLock.png"));
    }

    public override ICommandResult Invoke()
    {
        try
        {
            var sim = new InputSimulator();

            // Simulate holding down Left Windows key, Left Control key, Left Shift key, then pressing 'R'
            sim.Keyboard.ModifiedKeyStroke(new[] { VirtualKeyCode.LWIN, VirtualKeyCode.LCONTROL, VirtualKeyCode.LSHIFT }, VirtualKeyCode.VK_R);

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
