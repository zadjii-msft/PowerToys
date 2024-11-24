// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Windows.UI;

namespace NflExtension;

internal sealed partial class NflExtensionPage : ListPage
{
    internal static readonly HttpClient Client = new();
    internal static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public NflExtensionPage()
    {
        Icon = new("🏈");
        Name = "NFL Scores";
    }

    public override IListItem[] GetItems()
    {
        var dataAsync = FetchDataAsync();
        dataAsync.ConfigureAwait(false);
        var data = dataAsync.Result;
        return data.Events.Select(e => EventNodeToItem(e)).ToArray();
    }

    private ListItem EventNodeToItem(NflEvent e)
    {
        var name = e.Name;
        var game = e
                .Competitions
                    .First<Competition>();
        var tags = game
                    .Competitors
                    .Select(c =>
                        new Microsoft.CmdPal.Extensions.Helpers.Tag()
                        {
                            Icon = new IconDataType(c.Team.Id == game.Situation?.Possession ? "🏈" : string.Empty),
                            Text = $"{c.Team.Abbreviation} {c.Score}",
                            Color = HexToColor(c.Team.Color),
                        }).ToArray();
        return new ListItem(new NoOpCommand())
        {
            Title = name,
            Subtitle = e.Status.Type.Detail,
            Tags = tags,
        };
    }

    private static Color HexToColor(string hex)
    {
        // Ensure the string has the correct length
        if (hex.Length != 6 && hex.Length != 8)
        {
            throw new ArgumentException("Hex string must be 6 or 8 characters long.");
        }

        // If the string is 6 characters, assume it's RGB and prepend alpha as FF
        if (hex.Length == 6)
        {
            hex = "FF" + hex;
        }

        // Parse the hex values into bytes
        var a = Convert.ToByte(hex.Substring(0, 2), 16); // Alpha
        var r = Convert.ToByte(hex.Substring(2, 2), 16); // Red
        var g = Convert.ToByte(hex.Substring(4, 2), 16); // Green
        var b = Convert.ToByte(hex.Substring(6, 2), 16); // Blue

        return Color.FromArgb(a, r, g, b);
    }

    private async Task<NflData> FetchDataAsync()
    {
        try
        {
            // Make a GET request to the Mastodon trends API endpoint
            var response = await Client
                .GetAsync($"https://site.api.espn.com/apis/site/v2/sports/football/nfl/scoreboard?dates=20241124");
            response.EnsureSuccessStatusCode();

            // Read and deserialize the response JSON into a list of MastodonStatus objects
            var responseBody = await response.Content.ReadAsStringAsync();

            var data = JsonSerializer.Deserialize<NflData>(responseBody, Options);

            return data;
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occurred: {e.Message}");
        }

        return null;
    }
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This is sample code")]
public sealed class NflData
{
    [JsonPropertyName("events")]
    public List<NflEvent> Events { get; set; }
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This is sample code")]
public sealed class NflEvent
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("status")]
    public NflEventStatus Status { get; set; }

    [JsonPropertyName("competitions")]
    public List<Competition> Competitions { get; set; }
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This is sample code")]
public sealed class Competition
{
    [JsonPropertyName("competitors")]
    public List<Competitor> Competitors { get; set; }

    [JsonPropertyName("situation")]
    public CompetitionSituation Situation { get; set; } = null;
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This is sample code")]
public sealed class Competitor
{
    [JsonPropertyName("team")]
    public Team Team { get; set; }

    [JsonPropertyName("score")]
    public string Score { get; set; }
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This is sample code")]
public sealed class Team
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("abbreviation")]
    public string Abbreviation { get; set; }

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; }

    [JsonPropertyName("shortDisplayName")]
    public string ShortDisplayName { get; set; }

    [JsonPropertyName("color")]
    public string Color { get; set; }

    [JsonPropertyName("alternateColor")]
    public string AlternateColor { get; set; }
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This is sample code")]
public sealed class CompetitionSituation
{
    [JsonPropertyName("downDistanceText")]
    public string DownDistanceText { get; set; }

    [JsonPropertyName("shortDownDistanceText")]
    public string ShortDownDistanceText { get; set; }

    [JsonPropertyName("possessionText")]
    public string PossessionText { get; set; }

    [JsonPropertyName("isRedZone")]
    public bool IsRedZone { get; set; }

    [JsonPropertyName("homeTimeouts")]
    public int HomeTimeouts { get; set; }

    [JsonPropertyName("awayTimeouts")]
    public int AwayTimeouts { get; set; }

    [JsonPropertyName("possession")]
    public string Possession { get; set; }
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This is sample code")]
public sealed class NflEventStatus
{
    [JsonPropertyName("type")]
    public NflEventStatusType Type { get; set; }
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This is sample code")]
public sealed class NflEventStatusType
{
    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("detail")]
    public string Detail { get; set; }

    [JsonPropertyName("shortDetail")]
    public string ShortDetail { get; set; }
}
