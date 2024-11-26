// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Extensions.Helpers;
using Wox.Infrastructure;
using BrowserInfo = Wox.Plugin.Common.DefaultBrowserInfo;

namespace Microsoft.CmdPal.Ext.WebSearch.Commands;

internal sealed partial class OpenCommandInShell : InvokableCommand
{
    private readonly string _arguments;

    internal OpenCommandInShell(string arguments)
    {
        _arguments = arguments;
        BrowserInfo.UpdateIfTimePassed();
        Icon = new(BrowserInfo.IconPath);
        Name = Properties.Resources.open_in_default_browser;
    }

    public override CommandResult Invoke()
    {
        if (!Helper.OpenCommandInShell(BrowserInfo.Path, BrowserInfo.ArgumentsPattern, _arguments))
        {
            // TODO GH# 138 --> actually display feedback from the extension somewhere.
            return CommandResult.KeepOpen();
        }

        return CommandResult.Dismiss();
    }
}
