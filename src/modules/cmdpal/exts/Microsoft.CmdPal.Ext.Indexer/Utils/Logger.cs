// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace Microsoft.CmdPal.Ext.Indexer.Utils;

internal static class Logger
{
    private static readonly string Error = "Error";
    private static readonly string Warning = "Warning";
    private static readonly string Info = "Info";
    private static readonly string Debug = "Debug";
    private static readonly string TraceFlag = "Trace";

    public static void InitializeLogger()
    {
        var applicationLogPath = Environment.GetEnvironmentVariable("userprofile") + "\\AppData\\Local\\CmdPal";

        if (!Directory.Exists(applicationLogPath))
        {
            Directory.CreateDirectory(applicationLogPath);
        }

        var logFilePath = Path.Combine(applicationLogPath, "Log_" + DateTime.Now.ToString(@"yyyy-MM-dd", CultureInfo.InvariantCulture) + ".txt");

        Trace.Listeners.Add(new TextWriterTraceListener(logFilePath));

        Trace.AutoFlush = true;
    }

    public static void LogError(string message) => Log(message, Error);

    public static void LogError(string message, Exception ex)
    {
        if (ex == null)
        {
            LogError(message);
        }
        else
        {
            var exMessage =
                message + Environment.NewLine +
                ex.GetType() + ": " + ex.Message + Environment.NewLine;

            if (ex.InnerException != null)
            {
                exMessage +=
                    "Inner exception: " + Environment.NewLine +
                    ex.InnerException.GetType() + ": " + ex.InnerException.Message + Environment.NewLine;
            }

            exMessage +=
                "Stack trace: " + Environment.NewLine +
                ex.StackTrace;

            Log(exMessage, Error);
        }
    }

    public static void LogWarning(string message) => Log(message, Warning);

    public static void LogInfo(string message) => Log(message, Info);

    public static void LogDebug(string message) => Log(message, Debug);

    public static void LogTrace() => Log(string.Empty, TraceFlag);

    private static void Log(string message, string type)
    {
        if (message != string.Empty)
        {
            Trace.WriteLine("[" + DateTime.Now.TimeOfDay + "] [" + type + "] [" + GetCallerInfo() + "] " + message);
        }
    }

    private static string GetCallerInfo()
    {
        StackTrace stackTrace = new();

        var methodName = stackTrace.GetFrame(3)?.GetMethod();
        var className = methodName?.DeclaringType.Name;
        return className + "::" + methodName?.Name;
    }
}
