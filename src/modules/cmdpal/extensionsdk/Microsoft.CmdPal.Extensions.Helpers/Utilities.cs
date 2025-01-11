// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Windows.Foundation;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;

namespace Microsoft.CmdPal.Extensions.Helpers;

public class Utilities
{
    /// <summary>
    /// Used to produce a path to a settings folder which your app can use.
    /// If your app is running packaged, this will return the redirected local
    /// app data path (Packages/<your_pfn>/LocalState). If not, it'll return
    /// %LOCALAPPDATA%\settingsFolderName.
    /// </summary>
    /// <param name="settingsFolderName"></param>
    /// <returns></returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1312:Variable names should begin with lower-case letter", Justification = "It's a Windows constants a ")]
    public static string BaseSettingsPath(string settingsFolderName)
    {
        var FOLDERID_LocalAppData = new Guid("F1B32785-6FBA-4FCF-9D55-7B8E7F157091");
        var hr = PInvoke.SHGetKnownFolderPath(
            FOLDERID_LocalAppData,
            (uint)KNOWN_FOLDER_FLAG.KF_FLAG_FORCE_APP_DATA_REDIRECTION,
            null,
            out var localAppDataFolder);

        if (hr.Succeeded)
        {
            var basePath = new string(localAppDataFolder.ToString());
            if (!IsPackaged())
            {
                basePath = Path.Combine(basePath, settingsFolderName);
            }

            return basePath;
        }
        else
        {
            throw Marshal.GetExceptionForHR(hr.Value)!;
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1312:Variable names should begin with lower-case letter", Justification = "It's a Windows constants a ")]
    public static bool IsPackaged()
    {
        uint buffersize = 0;
        var bytes = Array.Empty<byte>();

        // CsWinRT apparently won't generate this constant
        var APPMODEL_ERROR_NO_PACKAGE = (WIN32_ERROR)15700;
        unsafe
        {
            fixed (byte* p = bytes)
            {
                var win32Error = PInvoke.GetCurrentPackageId(ref buffersize, p);
                return win32Error != APPMODEL_ERROR_NO_PACKAGE;
            }
        }
    }
}