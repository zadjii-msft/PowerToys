// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CmdPal.Ext.ClipboardHistory.Models;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Microsoft.Win32;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Streams;

namespace Microsoft.CmdPal.Ext.ClipboardHistory.Pages;

internal sealed partial class ClipboardHistoryListPage : ListPage
{
    private readonly ObservableCollection<ClipboardItem> clipboardHistory;
    private readonly string _defaultIconPath;

    public ClipboardHistoryListPage()
    {
        clipboardHistory = new ObservableCollection<ClipboardItem>();
        _defaultIconPath = string.Empty;
        Icon = new("\uF0E3"); // ClipboardList icon
        Name = "Clipboard History";
        ShowDetails = true;
    }

    private bool IsClipboardHistoryEnabled()
    {
        var registryKey = @"HKEY_CURRENT_USER\Software\Microsoft\Clipboard\";
        try
        {
            var enableClipboardHistory = (int)Registry.GetValue(registryKey, "EnableClipboardHistory", false);
            return enableClipboardHistory != 0;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private bool IsClipboardHistoryDisabledByGPO()
    {
        var registryKey = @"HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Windows\System\";
        try
        {
            var allowClipboardHistory = Registry.GetValue(registryKey, "AllowClipboardHistory", null);
            if (allowClipboardHistory != null)
            {
                return (int)allowClipboardHistory == 0;
            }
            else
            {
                return false;
            }
        }
        catch (Exception)
        {
            return false;
        }
    }

    private async Task LoadClipboardHistoryAsync()
    {
        try
        {
            List<ClipboardItem> items = new();

            if (Clipboard.IsHistoryEnabled())
            {
                var historyItems = await Clipboard.GetHistoryItemsAsync();
                if (historyItems.Status == ClipboardHistoryItemsResultStatus.Success)
                {
                    foreach (var item in historyItems.Items)
                    {
                        if (item.Content.Contains(StandardDataFormats.Text))
                        {
                            var text = await item.Content.GetTextAsync();
                            items.Add(new ClipboardItem { Content = text, Item = item });
                        }
                        else if (item.Content.Contains(StandardDataFormats.Bitmap))
                        {
                            items.Add(new ClipboardItem { Item = item });
                        }
                    }
                }
            }

            clipboardHistory.Clear();

            foreach (var item in items)
            {
                if (item.Item.Content.Contains(StandardDataFormats.Bitmap))
                {
                    IRandomAccessStreamReference imageReceived = await item.Item.Content.GetBitmapAsync();

                    if (imageReceived != null)
                    {
                        using var imageStream = await imageReceived.OpenReadAsync();
                        using var memoryStream = new MemoryStream();
                        await imageStream.AsStreamForRead().CopyToAsync(memoryStream);
                        item.ImageData = memoryStream.ToArray();
                    }
                }

                clipboardHistory.Add(item);
            }
        }
#pragma warning disable CS0168, IDE0059
        catch (Exception ex)
        {
            // TODO GH #108 We need to figure out some logging
            // Logger.LogError("Loading clipboard history failed", ex);
        }
#pragma warning restore CS0168, IDE0059
    }

    private async Task<ListItem[]> GetClipboardHistoryListItems()
    {
        await LoadClipboardHistoryAsync();
        ListItem[] listItems = new ListItem[clipboardHistory.Count];
        for (var i = 0; i < clipboardHistory.Count; i++)
        {
            var item = clipboardHistory[i];
            listItems[i] = item.ToListItem();
        }

        return listItems;
    }

    public override IListItem[] GetItems()
    {
        var t = DoGetItems();
        t.ConfigureAwait(false);
        return t.Result;
    }

    private async Task<IListItem[]> DoGetItems()
    {
        return await GetClipboardHistoryListItems();
    }
}
