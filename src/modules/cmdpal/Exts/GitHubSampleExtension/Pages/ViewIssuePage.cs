// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using GitHubSampleExtension.Data;
using Microsoft.CmdPal.Extensions.Helpers;

namespace GitHubSampleExtension.Pages;

internal sealed partial class ViewIssuePage : MarkdownPage
{
    private static readonly string OpenImagePath = "https://github.com/user-attachments/assets/e4144bc6-91dc-4de8-acad-5ddf4e574edf";
    private static readonly string ClosedImagePath = "https://github.com/user-attachments/assets/b8cfaa2e-5407-4594-a50f-7f252d8f6baf";

    private readonly GitHubIssue issue;

    public ViewIssuePage(GitHubIssue issue)
    {
        this.issue = issue;
        Name = "View";
        this.Title = issue.Title;
        Icon = new(issue.State == "open" ? OpenImagePath : ClosedImagePath);
    }

    public override string[] Bodies()
    {
        return [issue.Body];
    }
}
