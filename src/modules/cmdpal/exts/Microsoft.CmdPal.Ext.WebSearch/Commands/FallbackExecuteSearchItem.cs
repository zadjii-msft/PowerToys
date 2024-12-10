// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using System.Text;
using Microsoft.CmdPal.Ext.WebSearch.Helpers;
using Microsoft.CmdPal.Extensions.Helpers;
using BrowserInfo = Wox.Plugin.Common.DefaultBrowserInfo;

namespace Microsoft.CmdPal.Ext.WebSearch.Commands;

internal sealed partial class FallbackExecuteSearchItem : FallbackCommandItem
{
    private readonly OpenCommandInShell _executeItem;
    private static readonly CompositeFormat PluginOpen = System.Text.CompositeFormat.Parse(Properties.Resources.plugin_open);

    public FallbackExecuteSearchItem(SettingsManager settings)
        : base(new OpenCommandInShell(string.Empty, settings))
    {
        _executeItem = (OpenCommandInShell)this.Command!;
        Title = string.Empty;
        Subtitle = string.Format(CultureInfo.CurrentCulture, PluginOpen, BrowserInfo.Name ?? BrowserInfo.MSEdgeName);
        Icon = new("\uF6FA"); // WebSearch icon

        // TODO: this is a bug in the current POC. I don't think Fallback items
        // get icons set correctly.
        _executeItem.Icon = Icon;
    }

    public override void UpdateQuery(string query)
    {
        _executeItem.Arguments = $"? {query}";
        Title = query;
    }
}
