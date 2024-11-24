// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace NflExtension;

internal sealed partial class NflExtensionPage : ListPage
{
    internal static readonly HttpClient Client = new();

    public NflExtensionPage()
    {
        Icon = new(string.Empty);
        Name = "NFL Scores Extension";
    }

    public override IListItem[] GetItems()
    {
        var dataAsync = FetchDataAsync();
        dataAsync.ConfigureAwait(false);
        var data = dataAsync.Result;

        var events = data["events"]!.AsArray();
        return events.Select(node => EventNodeToItem(node)).ToArray();
    }

    private ListItem EventNodeToItem(JsonNode eventNode)
    {
        var name = eventNode["name"]!.ToString();
        var status = eventNode["status"].AsObject();
        var statusType = status["type"].AsObject();
        return new ListItem(new NoOpCommand())
        {
            Title = name,
            Subtitle = statusType["detail"]!.ToString(),
        };
    }

    private async Task<JsonObject> FetchDataAsync()
    {
        try
        {
            // Make a GET request to the Mastodon trends API endpoint
            var response = await Client
                .GetAsync($"https://site.api.espn.com/apis/site/v2/sports/football/nfl/scoreboard?dates=20241124");
            response.EnsureSuccessStatusCode();

            // Read and deserialize the response JSON into a list of MastodonStatus objects
            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonNode.Parse(responseBody).AsObject();
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occurred: {e.Message}");
        }

        return [];
    }
}
