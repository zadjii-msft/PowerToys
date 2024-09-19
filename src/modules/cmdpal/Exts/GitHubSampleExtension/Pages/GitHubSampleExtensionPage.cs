// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GitHubSampleExtension.Commands;
using GitHubSampleExtension.Data;
using GitHubSampleExtension.Pages;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace GitHubSampleExtension;

internal sealed partial class GitHubSampleExtensionPage : ListPage
{
    public GitHubSampleExtensionPage()
    {
        Icon = new("https://upload.wikimedia.org/wikipedia/commons/thumb/a/ae/Github-desktop-logo-symbol.svg/240px-Github-desktop-logo-symbol.svg.png");
        ShowDetails = true;
        Name = "GitHub Issues";
    }

    private static async Task<List<GitHubIssue>> GetGitHubIssues()
    {
        var issues = new List<GitHubIssue>();
        string result;
        string errorResult;
        try
        {
            var ghPath = @"gh";

            var processInfo = new ProcessStartInfo
            {
                FileName = ghPath,
                Arguments = "search issues --author \"@me\" --limit 50 --json title,number,url,state,body",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), // Set a valid working directory
            };

            using var process = Process.Start(processInfo);
            result = await process.StandardOutput.ReadToEndAsync();
            errorResult = await process.StandardError.ReadToEndAsync();
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                return new List<GitHubIssue> { new() { Title = errorResult, Number = -1, Url = string.Empty } };
            }

            issues = JsonSerializer.Deserialize<List<GitHubIssue>>(result);
        }
        catch (Exception ex)
        {
            return new List<GitHubIssue>
            {
                new()
                {
                    Title = ex.Message,
                    Number = -1,
                    Url = ex.Message,
                },
            };
        }

        return issues;
    }

    private async Task<ISection[]> DoGetItems()
    {
        List<GitHubIssue> items = await GetGitHubIssues();
        var s = new ListSection()
        {
            Title = "Issues",
            Items = items
                            .Select((issue) => new ListItem(new ViewIssuePage(issue))
                            {
                                Title = issue.Title,
                                Subtitle = issue.Number.ToString(CultureInfo.CurrentCulture),
                                Details = new Details() { Body = issue.Body },
                                MoreCommands = [
                                    new CommandContextItem(new OpenIssueCommand(issue)),
                                ],
                            })
                            .ToArray(),
        };
        return [s];
    }

    public override ISection[] GetItems()
    {
        var t = DoGetItems();
        t.ConfigureAwait(false);
        return t.Result;
    }
}
