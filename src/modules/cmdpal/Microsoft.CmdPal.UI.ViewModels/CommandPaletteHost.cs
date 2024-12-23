// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.CmdPal.Extensions;
using Windows.Foundation;

namespace Microsoft.CmdPal.UI.ViewModels;

public sealed partial class CommandPaletteHost : IExtensionHost
{
    // Static singleton, so that we can access this from anywhere
    // Post MVVM - this should probably be like, a dependency injection thing.
    public static CommandPaletteHost Instance { get; } = new();

    private static readonly GlobalLogPageContext _globalLogPageContext = new();

    private ulong _hostHwnd;

    public ulong HostingHwnd => _hostHwnd;

    public string LanguageOverride => string.Empty;

    public ObservableCollection<LogMessageViewModel> LogMessages { get; } = new();

    public IAsyncAction ShowStatus(IStatusMessage message)
    {
        Debug.WriteLine(message.Message);
        return Task.CompletedTask.AsAsyncAction();
    }

    public IAsyncAction HideStatus(IStatusMessage message)
    {
        return Task.CompletedTask.AsAsyncAction();
    }

    public IAsyncAction LogMessage(ILogMessage message)
    {
        Debug.WriteLine(message.Message);

        _ = Task.Run(() =>
        {
            var vm = new LogMessageViewModel(message, _globalLogPageContext);
            vm.SafeInitializePropertiesSynchronous();

            Task.Factory.StartNew(
                () =>
                {
                    LogMessages.Add(vm);
                },
                CancellationToken.None,
                TaskCreationOptions.None,
                _globalLogPageContext.Scheduler);
        });

        // We can't just make a LogMessageViewModel : ExtensionObjectViewModel
        // because we don't necessarily know the page context. Butts.
        return Task.CompletedTask.AsAsyncAction();
    }

    public void SetHostHwnd(ulong hostHwnd)
    {
        _hostHwnd = hostHwnd;
    }
}
