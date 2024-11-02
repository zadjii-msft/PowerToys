// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace MastodonExtension;

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This is sample code")]
internal sealed partial class MastodonExtensionPage : ListPage
{
    private static readonly HttpClient Client = new();
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public MastodonExtensionPage()
    {
        Icon = new("https://mastodon.social/packs/media/icons/android-chrome-36x36-4c61fdb42936428af85afdbf8c6a45a8.png");
        Name = "Mastodon";
    }

    public override IListItem[] GetItems()
    {
        var postsAsync = FetchExplorePage();
        postsAsync.ConfigureAwait(false);
        var posts = postsAsync.Result;
        return posts
            .Select(p => new ListItem(new NoOpCommand())
            {
                Title = p.ContentAsPlainText(),
                Subtitle = p.Account.Username,
                Icon = new(p.Account.Avatar),
                Tags = [
                    new Tag()
                    {
                        Icon = new("\ue734"), // FavoriteStar
                        Text = p.Favorites.ToString(CultureInfo.CurrentCulture),
                    },
                    new Tag()
                    {
                        Icon = new("\ue8ee"), // RepeatAll
                        Text = p.Boosts.ToString(CultureInfo.CurrentCulture),
                    },
                ],
            })
            .ToArray();
    }

    public async Task<List<MastodonStatus>> FetchExplorePage()
    {
        var statuses = new List<MastodonStatus>();

        try
        {
            // Make a GET request to the Mastodon trends API endpoint
            HttpResponseMessage response = await Client.GetAsync("https://mastodon.social/api/v1/trends/statuses");
            response.EnsureSuccessStatusCode();

            // Read and deserialize the response JSON into a list of MastodonStatus objects
            var responseBody = await response.Content.ReadAsStringAsync();
            statuses = JsonSerializer.Deserialize<List<MastodonStatus>>(responseBody, Options);
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occurred: {e.Message}");
        }

        return statuses;
    }
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This is sample code")]
public partial class MastodonExtensionActionsProvider : ICommandProvider
{
    public string DisplayName => $"Mastodon extension for cmdpal Commands";

    public IconDataType Icon => new(string.Empty);

    private readonly IListItem[] _actions = [
        new ListItem(new MastodonExtensionPage()),
    ];

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
    public void Dispose() => throw new NotImplementedException();
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize

    public IListItem[] TopLevelCommands()
    {
        return _actions;
    }
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This is sample code")]
public class MastodonStatus
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("account")]
    public MastodonAccount Account { get; set; }

    [JsonPropertyName("favourites_count")]
    public int Favorites { get; set; }

    [JsonPropertyName("reblogs_count")]
    public int Boosts { get; set; }

    [JsonPropertyName("replies_count")]
    public int Replies { get; set; }

    public string ContentAsPlainText()
    {
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(Content);
        StringBuilder plainTextBuilder = new StringBuilder();
        foreach (var node in doc.DocumentNode.ChildNodes)
        {
            plainTextBuilder.Append(ParseNodeToPlaintext(node));
        }

        return plainTextBuilder.ToString();
    }

    public string ContentAsMarkdown()
    {
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(Content);
        StringBuilder markdownBuilder = new StringBuilder();
        foreach (var node in doc.DocumentNode.ChildNodes)
        {
            markdownBuilder.Append(ParseNodeToMarkdown(node));
        }

        return markdownBuilder.ToString();
    }

    private static string ParseNodeToMarkdown(HtmlNode node)
    {
        switch (node.Name)
        {
            case "strong":
            case "b":
                return $"**{node.InnerText}**";
            case "em":
            case "i":
                return $"*{node.InnerText}*";
            case "a":
                return $"[{node.InnerText}]({node.GetAttributeValue("href", "#")})";
            case "p":
                return $"{node.InnerText}\n\n";
            case "#text":
                return node.InnerText;
            default:
                return node.InnerText;  // For unhandled nodes, just return the text.
        }
    }

    private static string ParseNodeToPlaintext(HtmlNode node)
    {
        return node.InnerText;
    }
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This is sample code")]
public class MastodonAccount
{
    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; }

    [JsonPropertyName("avatar")]
    public string Avatar { get; set; }
}
