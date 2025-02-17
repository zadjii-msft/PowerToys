﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Globalization;
using System.IO;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace Microsoft.CmdPal.Ext.TimeDate.Helpers;

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
        return !isSystemTimeDate
            ? Resources.ResourceManager.GetString(stringId, CultureInfo.CurrentUICulture) ?? string.Empty
            : !string.IsNullOrEmpty(stringIdNow)
                ? Resources.ResourceManager.GetString(stringIdNow, CultureInfo.CurrentUICulture) ?? string.Empty
                : Resources.ResourceManager.GetString(stringId + "Now", CultureInfo.CurrentUICulture) ?? string.Empty;
    }

    public static IconInfo TimeIcon { get; } = IconHelpers.FromRelativePaths(
        "Microsoft.CmdPal.Ext.TimeDate\\Assets\\time.light.png",
        "Microsoft.CmdPal.Ext.TimeDate\\Assets\\time.dark.png");

    public static IconInfo CalendarIcon { get; } = IconHelpers.FromRelativePaths(
        "Microsoft.CmdPal.Ext.TimeDate\\Assets\\calendar.light.png",
        "Microsoft.CmdPal.Ext.TimeDate\\Assets\\calendar.dark.png");

    public static IconInfo TimeDateIcon { get; } = IconHelpers.FromRelativePaths(
        "Microsoft.CmdPal.Ext.TimeDate\\Assets\\timeDate.light.png",
        "Microsoft.CmdPal.Ext.TimeDate\\Assets\\timeDate.dark.png");

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

    public static IconInfo CreateIconInfo(string iconName)
    {
        var rootPath = Path.Combine(AppContext.BaseDirectory, "Assets");
        var lightIconPath = Path.Combine(rootPath, $"{iconName}.light.png");
        var darkIconPath = Path.Combine(rootPath, $"{iconName}.dark.png");

        var lightIcon = new IconData(lightIconPath);
        var darkIcon = new IconData(darkIconPath);

        return new IconInfo(lightIcon, darkIcon);
    }
}
