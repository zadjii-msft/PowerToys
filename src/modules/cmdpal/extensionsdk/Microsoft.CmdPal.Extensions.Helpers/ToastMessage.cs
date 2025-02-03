// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.CmdPal.Extensions.Helpers;

public partial class ToastMessage
{
    private readonly Lock _showLock = new();
    private bool _shown;

    public StatusMessage Message { get; init; }

    public int Duration { get; init; } = 2500;

    public ToastMessage(StatusMessage message)
    {
        Message = message;
    }

    public ToastMessage(string text)
    {
        Message = new StatusMessage() { Message = text };
    }

    public void Show()
    {
        lock (_showLock)
        {
            if (!_shown)
            {
                ExtensionHost.ShowStatus(Message);
                _ = Task.Run(() =>
                {
                    Thread.Sleep(Duration);

                    lock (_showLock)
                    {
                        _shown = false;
                        ExtensionHost.HideStatus(Message);
                    }
                });
                _shown = true;
            }
        }
    }
}
