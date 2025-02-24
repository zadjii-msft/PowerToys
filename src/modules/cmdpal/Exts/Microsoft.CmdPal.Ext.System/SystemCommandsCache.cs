// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CmdPal.Ext.System.Helpers;
using Microsoft.CommandPalette.Extensions;

namespace Microsoft.CmdPal.Ext.System;

public sealed partial class SystemCommandsCache
{
    public SystemCommandsCache()
    {
        var list = new List<IListItem>();
        var listLock = new object();

        var a = Task.Run(() =>
        {
            var isBootedInUefiMode = Win32Helpers.GetSystemFirmwareType() == FirmwareType.Uefi;
            var separateEmptyRB = true;
            var confirmSystemCommands = true;
            var showSuccessOnEmptyRB = true;

            // normal system commands are fast and can be returned immediately
            var systemCommands = Commands.GetSystemCommands(isBootedInUefiMode, separateEmptyRB, confirmSystemCommands, showSuccessOnEmptyRB);
            lock (listLock)
            {
                list.AddRange(systemCommands);
            }
        });

        var b = Task.Run(() =>
        {
            var isBootedInUefiMode = Win32Helpers.GetSystemFirmwareType() == FirmwareType.Uefi;

            // Network (ip and mac) results are slow with many network cards and returned delayed.
            // On global queries the first word/part has to be 'ip', 'mac' or 'address' for network results
            var networkConnectionResults = Commands.GetNetworkConnectionResults();
            lock (listLock)
            {
                list.AddRange(networkConnectionResults);
            }
        });

        Task.WaitAll(a, b);
        CachedCommands = list.ToArray();
    }

    public IListItem[] CachedCommands { get; }
}
