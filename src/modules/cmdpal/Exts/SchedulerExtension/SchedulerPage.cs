// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml.Linq;
using Azure.Identity;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.TermStore;
using static System.Formats.Asn1.AsnWriter;

namespace SchedulerExtension;

internal sealed partial class SchedulerPage : ListPage
{
    public SchedulerPage()
    {
        Icon = new(string.Empty);
        Name = "Scheduler";
    }

    private static async Task<List<CmdPalToDo>> GetToDoTasks()
    {
        var scopes = new[] { "https://graph.microsoft.com/.default" };

        // Multi-tenant apps can use "common",
        // single-tenant apps must use the tenant ID from the Azure portal
        var tenantId = "tenentid";

        // Values from app registration
        var clientId = "clientid";
        var clientSecret = "clientsecret";

        // using Azure.Identity;
        var options = new OnBehalfOfCredentialOptions
        {
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
        };

        // This is the incoming token to exchange using on-behalf-of flow
        var oboToken = "JWT_TOKEN_TO_EXCHANGE";

        var onBehalfOfCredential = new OnBehalfOfCredential(
            tenantId, clientId, clientSecret, oboToken, options);

        var graphClient = new GraphServiceClient(onBehalfOfCredential, scopes);

        try
        {
            // GET / me / todo / lists /{ todoTaskListId}/ tasks

            // To initialize your graphClient, see https://learn.microsoft.com/en-us/graph/sdks/create-client?from=snippets&tabs=csharp
            var listsResult = await graphClient.Me.Todo.Lists.GetAsync();

            var listId = listsResult.Value.ToArray()[0].Id;

            var result = await graphClient.Me.Todo.Lists[listId].Tasks.GetAsync();
            var items = result.Value.ToArray().ToList();

            var tasks = new List<CmdPalToDo>();

            foreach (var item in items)
            {
                var task = new CmdPalToDo
                {
                    Title = item.Title,
                    Status = item.Status,
                    DueDateTime = item.DueDateTime,
                    ReminderDateTime = item.ReminderDateTime,
                    Recurrence = item.Recurrence,
                    Id = item.Id,
                    Body = item.Body,
                };
                tasks.Add(task);
            }

            return tasks;
        }
        catch (ServiceException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return new List<CmdPalToDo>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
            return new List<CmdPalToDo>();
        }
    }

    public override ISection[] GetItems()
    {
        var t = DoGetItems();
        t.ConfigureAwait(false);
        return t.Result;
    }

    private async Task<ISection[]> DoGetItems()
    {
        List<CmdPalToDo> toDos = await GetToDoTasks();
        this.Loading = false;
        var s = new ListSection()
        {
            Title = "All my tasks",
            Items = toDos.Select((cmdPalToDo) => new Microsoft.CmdPal.Extensions.Helpers.ListItem(new NoOpCommand())
            {
                Title = cmdPalToDo.Title,
                Subtitle = cmdPalToDo.Body.ToString(),
                MoreCommands = [new CommandContextItem(new NoOpCommand())],
            }).ToArray(),
        };

        return [s];
    }
}
