// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using Microsoft.CmdPal.Extensions.Helpers;

namespace MicrosoftLearnExtension;

internal sealed partial class LinkAction : InvokableCommand
{
    private readonly SearchResult _post;

    internal LinkAction(SearchResult post)
    {
        this._post = post;
        this.Name = "Open link";
        this.Icon = new("\uE8A7");
    }

    public override CommandResult Invoke()
    {
        Process.Start(new ProcessStartInfo(_post.Url) { UseShellExecute = true });
        return CommandResult.KeepOpen();
    }
}
