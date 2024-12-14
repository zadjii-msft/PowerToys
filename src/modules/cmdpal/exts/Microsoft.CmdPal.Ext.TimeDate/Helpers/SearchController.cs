// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CmdPal.Ext.TimeDate.Properties;
using Microsoft.CmdPal.Extensions.Helpers;

namespace Microsoft.CmdPal.Ext.TimeDate.Helpers;

/// <summary>
/// SearchController: Class to hold the search method that filter available date time formats
/// Extra class to simplify code in <see cref="Main"/> class
/// </summary>
internal static class SearchController
{
    /// <summary>
    /// Var that holds the delimiter between format and date
    /// </summary>
    private const string InputDelimiter = "::";

    /// <summary>
    /// Searches for results
    /// </summary>
    /// <param name="query">Search query object</param>
    /// <returns>List of Wox <see cref="Result"/>s.</returns>
    internal static List<ListItem> ExecuteSearch(string query)
    {
        var availableFormats = new List<AvailableResult>();
        List<ListItem> results = [];
        var isEmptySearchInput = string.IsNullOrEmpty(query);

        if (Regex.IsMatch(query, @".+" + Regex.Escape(InputDelimiter) + @".+"))
        {
            // Search for specified format with specified time/date value
            var userInput = query.Split(InputDelimiter);
            if (TimeAndDateHelper.ParseStringAsDateTime(userInput[1], out var timestamp))
            {
                availableFormats.AddRange(AvailableResultsList.GetList(null, null, timestamp));
                query = userInput[0];
            }
        }
        else if (TimeAndDateHelper.ParseStringAsDateTime(query, out var timestamp))
        {
            // Return all formats for specified time/date value
            availableFormats.AddRange(AvailableResultsList.GetList(null, null, timestamp));
            query = string.Empty;
        }
        else
        {
            // Search for specified format with system time/date (All other cases)
            availableFormats.AddRange(AvailableResultsList.GetList());
        }

        // Check searchTerm after getting results to select type of result list
        if (string.IsNullOrEmpty(query))
        {
            // Generate list with all results
            foreach (var f in availableFormats)
            {
                results.Add(new ListItem(new NoOpCommand())
                {
                    Title = f.Value ?? string.Empty,
                    Subtitle = $"{f.Label} - {Resources.Microsoft_plugin_timedate_SubTitleNote}",

                    // TODO FIX ICON
                    // IcoPath = f.GetIconPath(iconTheme),
                    // Action = _ => ResultHelper.CopyToClipBoard(f.Value),
                });
            }
        }
        else
        {
            // Generate filtered list of results
            foreach (var f in availableFormats)
            {
                var resultMatchScore = GetMatchScore(query, f.Label ?? string.Empty, f.AlternativeSearchTag ?? string.Empty);

                if (resultMatchScore > 0)
                {
                    results.Add(new ListItem(new NoOpCommand())
                    {
                        Title = f.Value ?? string.Empty,
                        Subtitle = $"{f.Label} - {Resources.Microsoft_plugin_timedate_SubTitleNote}",

                        // IcoPath = f.GetIconPath(iconTheme),

                        // Action = _ => ResultHelper.CopyToClipBoard(f.Value),
                        // Score = resultMatchScore,
                    });
                }
            }
        }

        // If search term is only a number that can't be parsed return an error message
        if (!isEmptySearchInput && results.Count == 0 && Regex.IsMatch(query, @"\w+\d+.*$") && !query.Any(char.IsWhiteSpace) && (TimeAndDateHelper.IsSpecialInputParsing(query) || !Regex.IsMatch(query, @"\d+[\.:/]\d+")))
        {
            results.Add(ResultHelper.CreateNumberErrorResult());
        }

        return results;
    }

    private static int GetMatchScore(string query, string label, string tags)
    {
        // Get match for label (or for tags if label score is <1)
        var score = StringMatcher.FuzzySearch(query, label).Score;
        if (score < 1)
        {
            foreach (var t in tags.Split(";"))
            {
                var tagScore = StringMatcher.FuzzySearch(query, t.Trim()).Score / 2;
                if (tagScore > score)
                {
                    score = tagScore / 2;
                }
            }
        }

        return score;
    }
}
