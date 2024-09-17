// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using static System.Formats.Asn1.AsnWriter;
using static System.Net.WebRequestMethods;

namespace SchedulerExtension;

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This is sample code")]
internal sealed class SchedulerExtensionPage : ListPage
{
    public SchedulerExtensionPage()
    {
        Icon = new(string.Empty);
        Name = "Scheduler extension for cmdpal";
    }

    public static async Task<List<CmdPalToDo>> GetToDoTasks()
    {
        // implement auth here
        var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

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

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This is sample code")]
public class SchedulerExtensionActionsProvider : ICommandProvider
{
    public string DisplayName => $"Scheduler extension for cmdpal Commands";

    public IconDataType Icon => new(string.Empty);

    private readonly IListItem[] _actions = [
        new Microsoft.CmdPal.Extensions.Helpers.ListItem(new SchedulerExtensionPage()),
    ];

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
    public void Dispose() => throw new NotImplementedException();
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize

    public IListItem[] TopLevelCommands()
    {
        return _actions;
    }
}
