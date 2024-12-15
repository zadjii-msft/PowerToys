﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.ObjectModel;

namespace Microsoft.CmdPal.Extensions.Helpers;

public class ListHelpers
{
    // Generate a score for a list item.
    public static int ScoreListItem(string query, ICommandItem listItem)
    {
        if (string.IsNullOrEmpty(query))
        {
            return 1;
        }

        var nameMatch = StringMatcher.FuzzySearch(query, listItem.Title);

        // var locNameMatch = StringMatcher.FuzzySearch(query, NameLocalized);
        var descriptionMatch = StringMatcher.FuzzySearch(query, listItem.Subtitle);

        // var executableNameMatch = StringMatcher.FuzzySearch(query, ExePath);
        // var locExecutableNameMatch = StringMatcher.FuzzySearch(query, ExecutableNameLocalized);
        // var lnkResolvedExecutableNameMatch = StringMatcher.FuzzySearch(query, LnkResolvedExecutableName);
        // var locLnkResolvedExecutableNameMatch = StringMatcher.FuzzySearch(query, LnkResolvedExecutableNameLocalized);
        // var score = new[] { nameMatch.Score, (descriptionMatch.Score - 4) / 2, executableNameMatch.Score }.Max();
        return new[] { nameMatch.Score, (descriptionMatch.Score - 4) / 2, 0 }.Max();
    }

    public static IEnumerable<IListItem> FilterList(IEnumerable<IListItem> items, string query)
    {
        var scores = items
            .Select(li => new ScoredListItem() { ListItem = li, Score = ScoreListItem(query, li) })
            .Where(score => score.Score > 0)
            .OrderByDescending(score => score.Score);
        return scores
            .Select(score => score.ListItem);
    }

    /// <summary>
    /// Modifies the contents of `original` in-place, to match those of
    /// `newContents`. The canonical use being:
    /// ```cs
    /// ListHelpers.InPlaceUpdateList(FilteredItems, FilterList(ItemsToFilter, TextToFilterOn));
    /// ```
    /// </summary>
    /// <typeparam name="T">Any type that can be compared for equality</typeparam>
    /// <param name="original">Collection to modify</param>
    /// <param name="newContents">The enumerable which `original` should match</param>
    public static void InPlaceUpdateList<T>(Collection<T> original, IEnumerable<T> newContents)
        where T : class
    {
        // Short circuit - new contents should just be empty
        if (!newContents.Any())
        {
            original.Clear();
            return;
        }

        var i = 0;
        foreach (var newItem in newContents)
        {
            if (i >= original.Count)
            {
                break;
            }

            for (var j = i; j < original.Count; j++)
            {
                var og_2 = original[j];
                var areEqual_2 = og_2.Equals(newItem);
                if (areEqual_2)
                {
                    for (var k = i; k < j; k++)
                    {
                        // This item from the original list was not in the new list. Remove it.
                        original.RemoveAt(i);
                    }

                    break;
                }
            }

            var og = original[i];
            var areEqual = og.Equals(newItem);

            // Is this new item already in the list?
            if (areEqual)
            {
                // It is already in the list
            }
            else
            {
                // it isn't. Add it.
                original.Insert(i, newItem);
            }

            i++;
        }

        // Remove any extra trailing items from the destination
        while (original.Count > newContents.Count())
        {
            // RemoveAtEnd
            original.RemoveAt(original.Count - 1);
        }

        // Add any extra trailing items from the source
        if (original.Count < newContents.Count())
        {
            var remaining = newContents.Skip(original.Count);
            foreach (var item in remaining)
            {
                original.Add(item);
            }
        }
    }
}

public struct ScoredListItem
{
    public int Score;
    public IListItem ListItem;
}
