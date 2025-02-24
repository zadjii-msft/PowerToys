// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.ComponentModel;

namespace Microsoft.CmdPal.UI.ViewModels;

public partial class RecentCommandsManager : ObservableObject
{
    private readonly List<HistoryItem> _history = [];

    public RecentCommandsManager()
    {
    }

    public int GetCommandHistoryWeight(string commandId)
    {
        var entry = _history
            .Index()
            .Where(item => item.Item.CommandId == commandId)
            .FirstOrDefault();

        // These numbers are vaguely scaled so that "VS" will make "Visual Studio" the
        // match after one use.
        // Usually it has a weight of 84, compared to 109 for the VS cmd prompt
        if (entry.Item != null)
        {
            var index = entry.Index;

            // First, add some weight based on how early in the list this appears
            var bucket = index switch
            {
                var i when index <= 2 => 35,
                var i when index <= 10 => 25,
                var i when index <= 15 => 15,
                var i when index <= 35 => 10,
                _ => 5,
            };

            // Then, add weight for how often this is used, but cap the weight from usage.
            var uses = Math.Min(entry.Item.Uses * 5, 35);

            return bucket + uses;
        }

        return 0;
    }

    public void AddHistoryItem(string commandId)
    {
        var entry = _history
            .Where(item => item.CommandId == commandId)
            .FirstOrDefault();
        if (entry == null)
        {
            var newitem = new HistoryItem() { CommandId = commandId, Uses = 1 };
            _history.Insert(0, newitem);
        }
        else
        {
            _history.Remove(entry);
            entry.Uses++;
            _history.Insert(0, entry);
        }

        if (_history.Count > 50)
        {
            _history.RemoveRange(50, _history.Count);
        }
    }
}
