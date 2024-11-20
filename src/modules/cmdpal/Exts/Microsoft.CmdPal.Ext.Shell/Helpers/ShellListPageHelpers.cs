// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CmdPal.Ext.Shell.Commands;
using Microsoft.CmdPal.Extensions.Helpers;

namespace Microsoft.CmdPal.Ext.Shell.Helpers;

public class ShellListPageHelpers
{
    private static readonly CompositeFormat WoxPluginCmdCmdHasBeenExecutedTimes = System.Text.CompositeFormat.Parse(Properties.Resources.wox_plugin_cmd_cmd_has_been_executed_times);
    private readonly ShellPluginSettings _settings;

    public ShellListPageHelpers()
    {
        _settings = new ShellPluginSettings();
    }

    private ListItem GetCurrentCmd(string cmd)
    {
        ListItem result = new ListItem(new ExecuteItem(cmd, _settings))
        {
            Title = cmd,
            Subtitle = Properties.Resources.wox_plugin_cmd_plugin_name + ": " + Properties.Resources.wox_plugin_cmd_execute_through_shell,
            Icon = new(string.Empty),
        };

        return result;
    }

    private List<ListItem> GetHistoryCmds(string cmd, ListItem result)
    {
        IEnumerable<ListItem> history = _settings.Count.Where(o => o.Key.Contains(cmd, StringComparison.CurrentCultureIgnoreCase))
            .OrderByDescending(o => o.Value)
            .Select(m =>
            {
                if (m.Key == cmd)
                {
                    // Using CurrentCulture since this is user facing
                    result.Subtitle = Properties.Resources.wox_plugin_cmd_plugin_name + ": " + string.Format(CultureInfo.CurrentCulture, WoxPluginCmdCmdHasBeenExecutedTimes, m.Value);
                    return null;
                }

                var ret = new ListItem(new ExecuteItem(m.Key, _settings))
                {
                    Title = m.Key,

                    // Using CurrentCulture since this is user facing
                    Subtitle = Properties.Resources.wox_plugin_cmd_plugin_name + ": " + string.Format(CultureInfo.CurrentCulture, WoxPluginCmdCmdHasBeenExecutedTimes, m.Value),
                    Icon = new(string.Empty),
                };
                return ret;
            }).Where(o => o != null).Take(4);
        return history.ToList();
    }

    public List<ListItem> Query(string query)
    {
        ArgumentNullException.ThrowIfNull(query);

        List<ListItem> results = new List<ListItem>();
        var cmd = query;
        if (string.IsNullOrEmpty(cmd))
        {
            return ResultsFromlHistory();
        }
        else
        {
            var queryCmd = GetCurrentCmd(cmd);
            results.Add(queryCmd);
            var history = GetHistoryCmds(cmd, queryCmd);
            results.AddRange(history);

            return results;
        }
    }

    private List<ListItem> ResultsFromlHistory()
    {
        IEnumerable<ListItem> history = _settings.Count.OrderByDescending(o => o.Value)
            .Select(m => new ListItem(new ExecuteItem(m.Key, _settings))
            {
                Title = m.Key,

                // Using CurrentCulture since this is user facing
                Subtitle = Properties.Resources.wox_plugin_cmd_plugin_name + ": " + string.Format(CultureInfo.CurrentCulture, WoxPluginCmdCmdHasBeenExecutedTimes, m.Value),
                Icon = new(string.Empty), // TODO GH #125 -- revisit Icons
            }).Take(5);
        return history.ToList();
    }
}
