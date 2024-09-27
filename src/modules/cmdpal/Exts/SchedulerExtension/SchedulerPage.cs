// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml.Linq;
using Azure.Core;
using Azure.Identity;
using Azure.Identity.Broker;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Microsoft.Extensions.Configuration;

using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;
using Microsoft.SqlServer.Server;
using Microsoft.UI.Windowing;
using Windows.ApplicationModel.Email.DataProvider;
using Windows.Media.Protection.PlayReady;
using Windows.Win32.Foundation;

// using Windows.Win32.UI.WindowsAndMessaging;
using static Microsoft.VisualStudio.Services.Graph.GraphResourceIds;
using Process = System.Diagnostics.Process;

namespace SchedulerExtension;

internal sealed partial class SchedulerPage : ListPage
{
    private string tenantId;
    private string clientId;
    private string[] scopes = new[] { "User.Read" };
    private TokenRequestContext tokenContext;
    private Uri redirectUri;
    private InteractiveBrowserCredential interactiveBrowserCredential;

    private enum GetAncestorFlags
    {
        GetParent = 1,
        GetRoot = 2,

        /// <summary>
        /// Retrieves the owned root window by walking the chain of parent and owner windows returned by GetParent.
        /// </summary>
        GetRootOwner = 3,
    }

    /// <summary>
    /// Retrieves the handle to the ancestor of the specified window.
    /// </summary>
    /// <param name="hwnd">A handle to the window whose ancestor is to be retrieved.
    /// If this parameter is the desktop window, the function returns NULL. </param>
    /// <param name="flags">The ancestor to be retrieved.</param>
    /// <returns>The return value is the handle to the ancestor window.</returns>
    [DllImport("user32.dll", ExactSpelling = true)]
    private static extern IntPtr GetAncestor(IntPtr hwnd, GetAncestorFlags flags);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    // This is your window handle!
    public IntPtr GetConsoleOrTerminalWindow()
    {
        var consoleHandle = GetForegroundWindow();
        if (consoleHandle != IntPtr.Zero)
        {
            var ancestorHandle = GetAncestor(consoleHandle, GetAncestorFlags.GetRootOwner);
            return ancestorHandle;
        }

        return IntPtr.Zero;
    }

    // https://learn.microsoft.com/en-us/graph/sdks/choose-authentication-providers?tabs=csharp
    public SchedulerPage()
    {
        Icon = new(string.Empty);
        Name = "Best scheduler ever.";
    }

    private void SetAuthInfo()
    {
        tenantId = "tenantId - actually principal domain";
        clientId = "clientId";
        scopes = new[] { "User.Read" };
        tokenContext = new TokenRequestContext(scopes);
        redirectUri = new Uri("ms-appx-web://microsoft.aad.brokerplugin/{clientId}");
    }

    private void SetBrowserCredential()
    {
        var parentWindowHandle = GetConsoleOrTerminalWindow();
        var brokerOptions = new InteractiveBrowserCredentialBrokerOptions(parentWindowHandle);
        brokerOptions.ClientId = clientId;
        brokerOptions.TenantId = tenantId;
        brokerOptions.AuthorityHost = AzureAuthorityHosts.AzurePublicCloud;
        brokerOptions.RedirectUri = redirectUri;
        interactiveBrowserCredential = new InteractiveBrowserCredential(brokerOptions);
    }

    private async Task<List<CmdPalToDo>> GetToDoTasks()
    {
        try
        {
            var graphServiceClient = new GraphServiceClient(interactiveBrowserCredential, scopes);
            var listsResult = await graphServiceClient.Me.Todo.Lists.GetAsync();
            var listId = listsResult.Value.ToArray()[0].Id;
            var result = await graphServiceClient.Me.Todo.Lists[listId].Tasks.GetAsync();
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
        SetAuthInfo();
        SetBrowserCredential();
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
