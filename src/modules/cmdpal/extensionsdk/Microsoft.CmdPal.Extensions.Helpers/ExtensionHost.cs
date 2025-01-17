// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.CmdPal.Extensions.Helpers;

public class ExtensionHost
{
    private static IExtensionHost? _host;

    public static IExtensionHost? Host => _host;

    public static void Initialize(IExtensionHost host)
    {
        _host = host;
    }

    /// <summary>
    /// Fire-and-forget a log message to the Command Palette host app. Since
    /// the host is in another process, we do this in a try/catch in a
    /// background thread, as to not block the calling thread, nor explode if
    /// the host app is gone.
    /// </summary>
    /// <param name="message">The log message to send</param>
    public static void LogMessage(ILogMessage message)
    {
        if (Host != null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await Host.LogMessage(message);
                }
                catch (Exception)
                {
                }
            });
        }
    }

    public static void LogMessage(string message)
    {
        var logMessage = new LogMessage() { Message = message };
        LogMessage(logMessage);
    }

    public static void ShowStatus(IStatusMessage message)
    {
        if (Host != null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await Host.ShowStatus(message);
                }
                catch (Exception)
                {
                }
            });
        }
    }
}
