// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Windows.UI.WebUI;

namespace MicrosoftLearnExtension;

internal sealed partial class MicrosoftLearnExtensionPage : DynamicListPage
{
    private static readonly string EndPointUrl = "https://learn.microsoft.com/api/search";

    // TODO: Provide options to search different locales and products
    private static readonly string Locale = "en-us";
    private static readonly string Product = "/devrel/b3627ad5-057f-488f-8b97-08be347b4be5"; // Windows App SDK

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public MicrosoftLearnExtensionPage()
    {
        Icon = new("https://learn.microsoft.com/favicon.ico");
        Name = "Microsoft Learn Doc Search";
        PlaceholderText = "Search Windows App SDK docs";
        IsLoading = true;

        // #091f2c
        AccentColor = ColorHelpers.FromRgb(9, 31, 44);
    }

    private static async Task<List<SearchResult>> SearchMicrosoftLearn(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new();
        }

        //// TODO: Sanitize input?
        using HttpClient client = new HttpClient();

        var response = await client.GetStringAsync($"{EndPointUrl}?locale={Locale}&$filter=products/any(product:product eq '{Product}')&search={query}");

        return JsonSerializer.Deserialize<SearchResultList>(response, JsonOptions).Results.ToList();
    }

    public override IListItem[] GetItems()
    {
        var t = DoGetItems(SearchText);
        t.ConfigureAwait(false);
        return t.Result;
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        IsLoading = true;
        GetItems();
    }

    private async Task<IListItem[]> DoGetItems(string query)
    {
        List<SearchResult> items = await SearchMicrosoftLearn(query);
        this.IsLoading = false;
        return items.Select((post) => new ListItem(new LinkAction(post))
        {
            Title = post.Title,
            Subtitle = post.Description,
        }).ToArray();
    }
}
