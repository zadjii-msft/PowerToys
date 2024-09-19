// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using GitHubSampleExtension.Data;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace GitHubSampleExtension.Commands;

internal sealed partial class OpenIssueCommand : InvokableCommand
{
    private readonly GitHubIssue issue;

    public OpenIssueCommand(GitHubIssue issue)
    {
        this.issue = issue;
        Name = "Open";
        Icon = new("\uE8A7");
    }

    public override ICommandResult Invoke()
    {
        Process.Start(new ProcessStartInfo(issue.Url) { UseShellExecute = true });
        return CommandResult.KeepOpen();
    }
}
