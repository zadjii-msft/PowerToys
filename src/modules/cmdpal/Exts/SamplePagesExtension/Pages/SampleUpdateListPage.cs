// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace SamplePagesExtension;

internal sealed partial class SampleUpdateListPage : DynamicListPage
{
    private bool _items;

    public SampleUpdateListPage()
    {
        Icon = new(string.Empty);
        Name = "SSH Keychain";

        // Start the background task to update the Items boolean every 5 seconds
        StartPeriodicUpdate();
    }

    // This method starts a Task that runs in the background and updates Items every 5 seconds
    private void StartPeriodicUpdate()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(5000);  // Wait for 5 seconds

                // Toggle the Items boolean
                _items = !_items;
                this.Items = _items;
            }
        });
    }

    public override ISection[] GetItems(string query)
    {
        // Change the list items based on the value of _items
        var listItems = _items
            ? new List<ListItem>
            {
                new(new NoOpCommand()) { Title = "Items is TRUE - Item 1", Subtitle = "This is dynamically generated content" },
                new(new NoOpCommand()) { Title = "Items is TRUE - Item 2", Subtitle = "This list updates every 5 seconds" },
            }
            : new List<ListItem>
            {
                new(new NoOpCommand()) { Title = "Items is FALSE - Item 1", Subtitle = "Items is currently false" },
                new(new NoOpCommand()) { Title = "Items is FALSE - Item 2", Subtitle = "The content will change again in 5 seconds" },
            };

        return new ISection[]
        {
            new ListSection()
            {
                Title = "Updating List Section",
                Items = listItems.ToArray(),
            },
        };
    }
}
