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

    private readonly List<ListItem> _items = new();

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
        if (_items.Count == 0)
        {
            // TODO: See if we can query this product list/portal section from "Product Documentation" menu in API?
            _items.Add(new ListItem(new LinkAction(new SearchResult()
            {
                Title = "Microsoft 365",
                Url = "https://learn.microsoft.com/microsoft-365/",
            }))
            {
                Title = "Microsoft 365",
            });
            _items.Add(new ListItem(new LinkAction(new SearchResult()
            {
                Title = "Artificial Intelligence",
                Url = "https://learn.microsoft.com/ai/",
            }))
            {
                Title = "Artificial Intelligence",
            });
            _items.Add(new ListItem(new LinkAction(new SearchResult()
            {
                Title = "Azure",
                Url = "https://learn.microsoft.com/azure/",
            }))
            {
                Title = "Azure",
            });
            _items.Add(new ListItem(new LinkAction(new SearchResult()
            {
                Title = "Microsoft Copilot",
                Url = "https://learn.microsoft.com/copilot/",
            }))
            {
                Title = "Microsoft Copilot",
            });
            _items.Add(new ListItem(new LinkAction(new SearchResult()
            {
                Title = ".NET",
                Url = "https://learn.microsoft.com/dotnet/",
            }))
            {
                Title = ".NET",
            });
            _items.Add(new ListItem(new LinkAction(new SearchResult()
            {
                Title = "PowerShell",
                Url = "https://learn.microsoft.com/powershell/",
            }))
            {
                Title = "PowerShell",
            });
            _items.Add(new ListItem(new LinkAction(new SearchResult()
            {
                Title = "Visual Studio",
                Url = "https://learn.microsoft.com/visualstudio/",
            }))
            {
                Title = "Visual Studio",
            });
            _items.Add(new ListItem(new LinkAction(new SearchResult()
            {
                Title = "Windows",
                Url = "https://learn.microsoft.com/windows/",
            }))
            {
                Title = "Windows",
            });
            _items.Add(new ListItem(new LinkAction(new SearchResult()
            {
                Title = "Windows Developer",
                Url = "https://developer.microsoft.com/windows/",
            }))
            {
                Title = "Windows Developer",
            });
            IsLoading = false;
        }

        return _items.ToArray();
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        IsLoading = true;

        Task.Run(async () =>
        {
            var items = await DoGetItems(newSearch);

            _items.Clear();
            _items.AddRange(items);

            IsLoading = false;

            RaiseItemsChanged(items.Length);
        });
    }

    private async Task<ListItem[]> DoGetItems(string query)
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
