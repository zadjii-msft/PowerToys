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

    public static MessageBoxResult Show(string text, string caption, IconType iconType, MessageBoxType type)
    {
        return (MessageBoxResult)MessageBox(IntPtr.Zero, text, caption, (int)type | (int)iconType);
    }

    public enum IconType
    {
        Error = 0x00000010,
        Help = 0x00000020,
        Warning = 0x00000030,
        Info = 0x00000040,
    }

    public enum MessageBoxType
    {
        OK = 0x00000000,
        OkAndCancel = 0x00000001,
        AboartRetryIgnore = 0x00000002,
        YesNoCancel = 0x00000003,
        YesOrNo = 0x00000004,
        RetryAndCancel = 0x00000005,
        CancelTryAgainAndContinue = 0x00000006,
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
