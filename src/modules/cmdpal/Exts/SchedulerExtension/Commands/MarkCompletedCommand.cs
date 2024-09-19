// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.CmdPal.Extensions.Helpers;
using SchedulerExtension.Data;

namespace SchedulerExtension.Commands;

internal sealed partial class MarkCompletedCommand : InvokableCommand
{
    private readonly ToDoItem _todoItem;
    private readonly SchedulerExtensionPage _page;

    internal MarkCompletedCommand(ToDoItem todoItem, SchedulerExtensionPage page)
    {
        this._todoItem = todoItem;
        this._page = page;
        this.Name = "Mark as Completed";
        this.Icon = new("\uE8A7");
    }

    public override CommandResult Invoke()
    {
        var t = InvokeAsync();
        t.ConfigureAwait(false);
        return t.Result;
    }

    private async Task<CommandResult> InvokeAsync()
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SchedulerExtensionPage._accessToken);

            var requestUri = $"https://graph.microsoft.com/v1.0/me/todo/lists/{_todoItem.ListId}/tasks/{_todoItem.Id}";

            var updateData = new
            {
                status = "completed", // If you're marking as complete
            };

            var content = new StringContent(JsonSerializer.Serialize(updateData), Encoding.UTF8, "application/json");

            // Create the PATCH request
            var method = new HttpMethod("PATCH");
            var request = new HttpRequestMessage(method, requestUri)
            {
                Content = content,
            };

            HttpResponseMessage response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                _page.UpdateItems();
                Console.WriteLine("Task updated successfully!");
            }
            else
            {
                Console.WriteLine($"Failed to update task. Status Code: {response.StatusCode}");
            }
        }
        catch
        {
            Console.WriteLine("Failed to mark the task as completed.");
        }

        return CommandResult.KeepOpen();
    }
}
