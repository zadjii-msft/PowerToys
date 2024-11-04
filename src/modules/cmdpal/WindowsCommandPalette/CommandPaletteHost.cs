// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.CmdPal.Common.Services;
using Microsoft.CmdPal.Extensions;
using Windows.Win32;

namespace WindowsCommandPalette;

public sealed partial class CommandPaletteHost : IExtensionHost
{
    public static CommandPaletteHost Instance { get; } = new();

    public ulong HostingHwnd => 12345u;

    public string LanguageOverride => throw new NotImplementedException();

    public void LogMessage(ILogMessage message) => throw new NotImplementedException();
}
