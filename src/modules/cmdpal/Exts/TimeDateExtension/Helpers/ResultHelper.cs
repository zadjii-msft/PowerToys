// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Microsoft.PowerToys.Run.Plugin.TimeDate.Properties;

namespace TimeDateExtension.Helpers;

internal static class ResultHelper
{
    /// <summary>
    /// Get the string based on the requested type
    /// </summary>
    /// <param name="isSystemTimeDate">Does the user search for system date/time?</param>
    /// <param name="stringId">Id of the string. (Example: `MyString` for `MyString` and `MyStringNow`)</param>
    /// <param name="stringIdNow">Optional string id for now case</param>
    /// <returns>The string from the resource file, or <see cref="string.Empty"/> otherwise.</returns>
    internal static string SelectStringFromResources(bool isSystemTimeDate, string stringId, string stringIdNow = default)
    {
        if (!isSystemTimeDate)
        {
            return Resources.ResourceManager.GetString(stringId, CultureInfo.CurrentUICulture) ?? string.Empty;
        }
        else if (!string.IsNullOrEmpty(stringIdNow))
        {
            return Resources.ResourceManager.GetString(stringIdNow, CultureInfo.CurrentUICulture) ?? string.Empty;
        }
        else
        {
            return Resources.ResourceManager.GetString(stringId + "Now", CultureInfo.CurrentUICulture) ?? string.Empty;
        }
    }

    /// <summary>
    /// Gets a result with an error message that only numbers can't be parsed
    /// </summary>
    /// <returns>Element of type <see cref="Result"/>.</returns>
    internal static ListItem CreateNumberErrorResult() => new ListItem(new NoOpCommand())
    {
        Title = Resources.Microsoft_plugin_timedate_ErrorResultTitle,
        Subtitle = Resources.Microsoft_plugin_timedate_ErrorResultSubTitle,
        Icon = CreateIconInfo("Warning"),
    };

    public static IconInfo CreateIconInfo(string inconType)
    {
        var lightIcon = new IconData($"Assets\\{inconType}.light.png");
        var darkIcon = new IconData($"Assets\\{inconType}.dark.png");

        return new IconInfo(lightIcon, darkIcon);
    }
}
