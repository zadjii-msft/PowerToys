// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;
using Microsoft.Windows.AppLifecycle;

namespace Microsoft.CmdPal.UI;

// cribbed heavily from
//
// https://github.com/microsoft/WindowsAppSDK-Samples/tree/main/Samples/AppLifecycle/Instancing/cs2/cs-winui-packaged/CsWinUiDesktopInstancing
internal sealed class Program
{
    private static App? app;

    // LOAD BEARING
    //
    // Main cannot be async. If it is, then the clipboard won't work, and neither will narrator.
    [STAThread]
    private static int Main(string[] args)
    {
        if (Helpers.GpoValueChecker.GetConfiguredCmdPalEnabledValue() == Helpers.GpoRuleConfiguredValue.Disabled)
        {
            // There's a GPO rule configured disabling CmdPal. Exit as soon as possible.
            return 0;
        }

        WinRT.ComWrappersSupport.InitializeComWrappers();
        var isRedirect = DecideRedirection();
        if (!isRedirect)
        {
            Microsoft.UI.Xaml.Application.Start((p) =>
            {
                var context = new Microsoft.UI.Dispatching.DispatcherQueueSynchronizationContext(Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(context);
                app = new App();
            });
        }

        return 0;
    }

    private static bool DecideRedirection()
    {
        var isRedirect = false;
        var args = AppInstance.GetCurrent().GetActivatedEventArgs();
        var keyInstance = AppInstance.FindOrRegisterForKey("randomKey");

        if (keyInstance.IsCurrent)
        {
            keyInstance.Activated += OnActivated;
        }
        else
        {
            isRedirect = true;
            RedirectActivationTo(args, keyInstance);

            // keyInstance.RedirectActivationToAsync(args).AsTask().ConfigureAwait(false);
        }

        return isRedirect;
    }

    private static void OnActivated(object? sender, AppActivationArguments args)
    {
        // If we already have a form, display the message now.
        // Otherwise, add it to the collection for displaying later.
        if (App.Current is App thisApp)
        {
            if (thisApp.AppWindow is not null and
                MainWindow mainWindow)
            {
                mainWindow.Summon(string.Empty);
            }
        }
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateEvent(
    IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string? lpName);

    [DllImport("kernel32.dll")]
    private static extern bool SetEvent(IntPtr hEvent);

    [DllImport("ole32.dll")]
    private static extern uint CoWaitForMultipleObjects(
    uint dwFlags, uint dwMilliseconds, ulong nHandles, IntPtr[] pHandles, out uint dwIndex);

    private static IntPtr redirectEventHandle = IntPtr.Zero;

    // Do the redirection on another thread, and use a non-blocking
    // wait method to wait for the redirection to complete.
    public static void RedirectActivationTo(AppActivationArguments args, AppInstance keyInstance)
    {
        redirectEventHandle = CreateEvent(IntPtr.Zero, true, false, null);
        Task.Run(() =>
        {
            keyInstance.RedirectActivationToAsync(args).AsTask().Wait();
            SetEvent(redirectEventHandle);
        });
        uint cWMO_DEFAULT = 0;
        var iNFINITE = 0xFFFFFFFF;
        _ = CoWaitForMultipleObjects(cWMO_DEFAULT, iNFINITE, 1, new IntPtr[] { redirectEventHandle }, out var handleIndex);
    }
}
