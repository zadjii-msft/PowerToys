// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace TimeDateExtension.Helpers;

/// <summary>
/// Additional settings for the WindowWalker plugin.
/// </summary>
/// <remarks>Some code parts reused from TimeZone plugin.</remarks>
internal sealed class TimeDateSettings
{
    /// <summary>
    /// An instance of the class <see cref="TimeDateSettings"></see>
    /// </summary>
    private static TimeDateSettings instance;

    /// <summary>
    /// Gets the value of the "First Week Rule" setting
    /// </summary>
    internal int CalendarFirstWeekRule { get; private set; }

    /// <summary>
    /// Gets the value of the "First Day Of Week" setting
    /// </summary>
    internal int FirstDayOfWeek { get; private set; }

    /// <summary>
    /// Gets a value indicating whether to show only the time and date for system time in global results or not
    /// </summary>
    internal bool OnlyDateTimeNowGlobal { get; private set; }

    /// <summary>
    /// Gets a value indicating whether to show the time with seconds or not
    /// </summary>
    internal bool TimeWithSeconds { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the date with the weekday or not
    /// </summary>
    internal bool DateWithWeekday { get; private set; }

    /// <summary>
    /// Gets a value indicating whether to hide the number input error message on global results
    /// </summary>
    internal bool HideNumberMessageOnGlobalQuery { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeDateSettings"/> class.
    /// Private constructor to make sure there is never more than one instance of this class
    /// </summary>
    private TimeDateSettings()
    {
        // Init class properties with default values
        CalendarFirstWeekRule = -1;
        FirstDayOfWeek = -1;
        OnlyDateTimeNowGlobal = true;
        TimeWithSeconds = false;
        DateWithWeekday = false;
        HideNumberMessageOnGlobalQuery = false;
    }

    /// <summary>
    /// Gets an instance property of this class that makes sure that the first instance gets created
    /// and that all the requests end up at that one instance.
    /// The benefit of this is that we don't need additional variables/parameters
    /// to communicate the settings between plugin's classes/methods.
    /// We can simply access this one instance, whenever we need the actual settings.
    /// </summary>
    internal static TimeDateSettings Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new TimeDateSettings();
            }

            return instance;
        }
    }
}
