// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using SchedulerExtension.Commands;
using SchedulerExtension.Data;

namespace SchedulerExtension;

internal sealed partial class SchedulerExtensionPage : ListPage
{
    public static readonly string _accessToken = "eyJ0eXAiOiJKV1QiLCJub25jZSI6ImZaUng0OUJ5WF9vbkNiVGJyN0NRWEU5dWNHakU5RnhfcHZWbmN5MHdRZmciLCJhbGciOiJSUzI1NiIsIng1dCI6Ikg5bmo1QU9Tc3dNcGhnMVNGeDdqYVYtbEI5dyIsImtpZCI6Ikg5bmo1QU9Tc3dNcGhnMVNGeDdqYVYtbEI5dyJ9.eyJhdWQiOiIwMDAwMDAwMy0wMDAwLTAwMDAtYzAwMC0wMDAwMDAwMDAwMDAiLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC83MmY5ODhiZi04NmYxLTQxYWYtOTFhYi0yZDdjZDAxMWRiNDcvIiwiaWF0IjoxNzI2Njc3MDc3LCJuYmYiOjE3MjY2NzcwNzcsImV4cCI6MTcyNjY4MjcyMCwiYWNjdCI6MCwiYWNyIjoiMSIsImFjcnMiOlsidXJuOnVzZXI6cmVnaXN0ZXJzZWN1cml0eWluZm8iLCJjMTAiXSwiYWlvIjoiQVlRQWUvOFhBQUFBYU5ibTdzbUhadnpJeDdSTDBaamFGbEVQZUNpU2FKTm9ha0ZneUZ4S1l5YnRhOXp4bEhadCtHYSt4bnFTNm5NSi9UbGNyT3JuVmNSeGVjYnY1b0o5d0lZQjRmcFp1UVdNaVpOZFcrOXNGLzBZRGs3QlE2b3Vvd1ZCbzJnYlY3dFovWG4valdBYnFQRE84cGdKbGpyVFhSOFlkYyt2dFZ5SWZpN2JHYjRRZ2JRPSIsImFtciI6WyJyc2EiLCJtZmEiXSwiYXBwX2Rpc3BsYXluYW1lIjoiR3JhcGggRXhwbG9yZXIiLCJhcHBpZCI6ImRlOGJjOGI1LWQ5ZjktNDhiMS1hOGFkLWI3NDhkYTcyNTA2NCIsImFwcGlkYWNyIjoiMCIsImNhcG9saWRzX2xhdGViaW5kIjpbIjU5NTZmZjVhLTZmZGItNDc3ZS05ZDRkLTlmN2QyNjJlNjk0YSJdLCJjb250cm9scyI6WyJhcHBfcmVzIl0sImNvbnRyb2xzX2F1ZHMiOlsiZGU4YmM4YjUtZDlmOS00OGIxLWE4YWQtYjc0OGRhNzI1MDY0IiwiMDAwMDAwMDMtMDAwMC0wMDAwLWMwMDAtMDAwMDAwMDAwMDAwIiwiMDAwMDAwMDMtMDAwMC0wZmYxLWNlMDAtMDAwMDAwMDAwMDAwIl0sImRldmljZWlkIjoiOGY3YWVjMzMtMDEzMC00ZjY3LWE1YjktYTQ4NzNjNTk3YzdmIiwiZmFtaWx5X25hbWUiOiJBZG91bWllIiwiZ2l2ZW5fbmFtZSI6IkpvcmRpIiwiaWR0eXAiOiJ1c2VyIiwiaXBhZGRyIjoiNC4xNTUuMC4xMDAiLCJuYW1lIjoiSm9yZGkgQWRvdW1pZSIsIm9pZCI6IjBjNDAwNTk4LTQ1ZDYtNDljOC1hMTdhLTAxMjliZDlmZWFmMCIsIm9ucHJlbV9zaWQiOiJTLTEtNS0yMS0yMTI3NTIxMTg0LTE2MDQwMTI5MjAtMTg4NzkyNzUyNy02NDMwODg4NSIsInBsYXRmIjoiMyIsInB1aWQiOiIxMDAzMjAwMjY4RjczRDk4IiwicmgiOiIxLkFSb0F2NGo1Y3ZHR3IwR1JxeTE4MEJIYlJ3TUFBQUFBQUFBQXdBQUFBQUFBQUFBYUFFRWFBQS4iLCJzY3AiOiJDYWxlbmRhcnMuUmVhZFdyaXRlIENvbnRhY3RzLlJlYWRXcml0ZSBEZXZpY2VNYW5hZ2VtZW50QXBwcy5SZWFkV3JpdGUuQWxsIERldmljZU1hbmFnZW1lbnRDb25maWd1cmF0aW9uLlJlYWQuQWxsIERldmljZU1hbmFnZW1lbnRDb25maWd1cmF0aW9uLlJlYWRXcml0ZS5BbGwgRGV2aWNlTWFuYWdlbWVudE1hbmFnZWREZXZpY2VzLlByaXZpbGVnZWRPcGVyYXRpb25zLkFsbCBEZXZpY2VNYW5hZ2VtZW50TWFuYWdlZERldmljZXMuUmVhZC5BbGwgRGV2aWNlTWFuYWdlbWVudE1hbmFnZWREZXZpY2VzLlJlYWRXcml0ZS5BbGwgRGV2aWNlTWFuYWdlbWVudFJCQUMuUmVhZC5BbGwgRGV2aWNlTWFuYWdlbWVudFJCQUMuUmVhZFdyaXRlLkFsbCBEZXZpY2VNYW5hZ2VtZW50U2VydmljZUNvbmZpZy5SZWFkLkFsbCBEZXZpY2VNYW5hZ2VtZW50U2VydmljZUNvbmZpZy5SZWFkV3JpdGUuQWxsIERpcmVjdG9yeS5BY2Nlc3NBc1VzZXIuQWxsIERpcmVjdG9yeS5SZWFkV3JpdGUuQWxsIEZpbGVzLlJlYWRXcml0ZS5BbGwgR3JvdXAuUmVhZFdyaXRlLkFsbCBJZGVudGl0eVJpc2tFdmVudC5SZWFkLkFsbCBNYWlsLlJlYWRXcml0ZSBNYWlsYm94U2V0dGluZ3MuUmVhZFdyaXRlIE5vdGVzLlJlYWRXcml0ZS5BbGwgb3BlbmlkIFBlb3BsZS5SZWFkIFBvbGljeS5SZWFkLkFsbCBQcmVzZW5jZS5SZWFkIFByZXNlbmNlLlJlYWQuQWxsIHByb2ZpbGUgUmVwb3J0cy5SZWFkLkFsbCBTaXRlcy5SZWFkV3JpdGUuQWxsIFRhc2tzLlJlYWRXcml0ZSBVc2VyLlJlYWQgVXNlci5SZWFkQmFzaWMuQWxsIFVzZXIuUmVhZFdyaXRlIFVzZXIuUmVhZFdyaXRlLkFsbCBlbWFpbCIsInNpZ25pbl9zdGF0ZSI6WyJkdmNfbW5nZCIsImR2Y19jbXAiLCJrbXNpIl0sInN1YiI6Ill5dS0tbVJWMDI2eVlBRTE5U002MUdZNXpGenAzX0tQUDQtXzNlVC13Z1UiLCJ0ZW5hbnRfcmVnaW9uX3Njb3BlIjoiV1ciLCJ0aWQiOiI3MmY5ODhiZi04NmYxLTQxYWYtOTFhYi0yZDdjZDAxMWRiNDciLCJ1bmlxdWVfbmFtZSI6ImpvcmRpYWRvdW1pZUBtaWNyb3NvZnQuY29tIiwidXBuIjoiam9yZGlhZG91bWllQG1pY3Jvc29mdC5jb20iLCJ1dGkiOiJZTDRtSDZna0NrR2RWWGJSRjNUWEFBIiwidmVyIjoiMS4wIiwid2lkcyI6WyJiNzlmYmY0ZC0zZWY5LTQ2ODktODE0My03NmIxOTRlODU1MDkiXSwieG1zX2NjIjpbIkNQMSJdLCJ4bXNfaWRyZWwiOiIyMiAxIiwieG1zX3NzbSI6IjEiLCJ4bXNfc3QiOnsic3ViIjoiemZJODdYSkJDZmZuZFp2OHMtdHZNRGFhUzV0RG9XT29GYkRVYURTX0NoSSJ9LCJ4bXNfdGNkdCI6MTI4OTI0MTU0N30.dEUTX0hq6qzKhxVPDCRvqYYV7e7Gcvv2ZucKWwAdZCIyX3UMUfWUl_jHlm6dpyXs7hRFcW3Pp3MaGsZLXS7VVv-s0Vlb7HZYgD5X0Q9XD8K1Pw3X0XGAzS5xjjg1iKojXqlBvASUeFAQZ0gAbHZEpfbn6km4awowL68mO5AmQDvXTbqyMq3PI97vicZYJKDeZZB3jVfU-Ku9mj7hUye0jAyQwo21YeSD6oaFlFVe82uPQg7n8L8w_Li_V9zSjHfBMPQlQgqxVO8iGnylrEihzQdxiZJ9ajuA8VrT9eX1lF-km9jbTsC82bI2M8G2L4oezGSjIyQRmp8SbU0aYtMK5A";
    private static readonly string _listId = "AQMkAGYyZDE1NDQANy1lOTY1LTRmYTAtYjRmNS1mNmY1ODkwNGUyM2EALgAAAz4hI6chBx1FhkbgPvsLlRsBALH1gVUYj9BDhuAn8cFI7BUAAAIBEgAAAA==";

    private bool _items;

    public SchedulerExtensionPage()
    {
        Icon = new("https://res.cdn.office.net/todo/2151454_2.125.2/favicon.ico");
        Name = "Search TODOs";
    }

    public void UpdateItems()
    {
        _items = !_items;
        this.Items = _items;
    }

    private sealed class JsonResponse
    {
        [JsonPropertyName("value")]
        public List<ToDoItem> Value { get; set; }
    }

    // Create a static JsonSerializerOptions instance
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static async Task<List<ToDoItem>> GetToDos()
    {
        var todos = new List<ToDoItem>();

        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            var requestUri = $"https://graph.microsoft.com/v1.0/me/todo/lists/{_listId}/tasks";

            var response = await client.GetStringAsync(requestUri);

            var jsonResponse = JsonSerializer.Deserialize<JsonResponse>(response, JsonOptions);

            if (jsonResponse != null)
            {
                foreach (var task in jsonResponse.Value)
                {
                    todos.Add(new ToDoItem
                    {
                        Title = task.Title,
                        CreatedDateTime = task.CreatedDateTime,
                        LastModifiedDateTime = task.LastModifiedDateTime,
                        Status = task.Status,
                        Id = task.Id,
                        ListId = _listId,
                    });
                }
            }
        }

        return todos;
    }

    private async Task<ISection[]> DoGetItems()
    {
        List<ToDoItem> items = await GetToDos();

        var completedItems = items.Where(todo => todo.Status == "completed").Select(todo => new ListItem(new MarkIncompleteCommand(todo, this))
        {
            Title = todo.Title,
            Subtitle = "Completed",
        }).ToArray();

        var notStartedItems = items.Where(todo => todo.Status == "notStarted").Select(todo => new ListItem(new MarkCompletedCommand(todo, this))
        {
            Title = todo.Title,
            Subtitle = todo.Status,
        }).ToArray();

        return new ISection[]
        {
            new ListSection()
            {
                Title = "ToDos",
                Items = notStartedItems,
            },
            new ListSection()
            {
                Title = "Completed",
                Items = completedItems,
            },
        };
    }

    public override ISection[] GetItems()
    {
        var t = DoGetItems();
        t.ConfigureAwait(false);
        return t.Result;
    }
}
