// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.CmdPal.Ext.System.Helpers;

public sealed partial class MessageBoxHelper
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBox(IntPtr hWnd, string text, string caption, int type);

    public static MessageBoxResult Show(string text, string caption, MessageBoxType type)
    {
        return (MessageBoxResult)MessageBox(IntPtr.Zero, text, caption, (int)type);
    }

    public enum MessageBoxType
    {
        OK = 0x00000000,
        OkAndCancel = 0x00000001,
        YesOrNo = 0x00000004,
    }

    public enum MessageBoxResult
    {
        OK = 1,
        Cancel = 2,
        Abort = 3,
        Retry = 4,
        Ignore = 5,
        Yes = 6,
        No = 7,
    }
}
