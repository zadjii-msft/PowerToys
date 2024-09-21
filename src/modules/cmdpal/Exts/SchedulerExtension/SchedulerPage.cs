// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml.Linq;
using Azure.Core;
using Azure.Identity;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Microsoft.Extensions.Configuration;

using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using Microsoft.IdentityModel.Abstractions;
using Microsoft.SqlServer.Server;
using Windows.ApplicationModel.Email.DataProvider;
using Windows.Media.Protection.PlayReady;
using static Microsoft.VisualStudio.Services.Graph.GraphResourceIds;

namespace SchedulerExtension;

internal sealed partial class SchedulerPage : ListPage
{
    // https://learn.microsoft.com/en-us/graph/sdks/choose-authentication-providers?tabs=csharp
    public SchedulerPage()
    {
        Icon = new(string.Empty);
        Name = "Scheduler";
    }

    private static async Task<List<CmdPalToDo>> GetToDoTasks()
    {
        // Multi-tenant apps can use "common",
        // single-tenant apps must use the tenant ID from the Azure portal
        // In the azure portal the value you need here is the Primary domain rather than the Tenant Id
        var tenantId = "tenantId";

        // Value from app registration
        var clientId = "clientId";

        var scopes = new[] { "User.Read" };

        // using Azure.Identity;
        var options = new InteractiveBrowserCredentialOptions
        {
            TenantId = tenantId,
            ClientId = clientId,
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,

            // MUST be http://localhost or http://localhost:PORT
            // See https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/System-Browser-on-.Net-Core
            RedirectUri = new Uri("http://localhost"),
        };
        try
        {
            var tokenContext = new TokenRequestContext(scopes);

            // https://learn.microsoft.com/dotnet/api/azure.identity.interactivebrowsercredential
            var interactiveCredential = new InteractiveBrowserCredential(options);
            var token = interactiveCredential.Authenticate(tokenContext);

            // interactiveCredential.
            var graphClient = new GraphServiceClient(interactiveCredential, scopes);

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
            System.Diagnostics.Debug.WriteLine(ex);

            // Handle specific Graph API exceptions
            Console.WriteLine($"Graph API error: {ex.Message}");

            // Handle the exception
            Console.WriteLine($"An error occurred: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);

            // Handle specific Graph API exceptions
            Console.WriteLine($"Graph API error: {ex.Message}");

            // Handle the exception
            Console.WriteLine($"An error occurred: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
        }

        return null;
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
