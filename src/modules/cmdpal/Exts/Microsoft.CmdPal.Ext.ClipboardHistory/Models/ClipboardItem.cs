// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Globalization;
using Microsoft.CmdPal.Ext.ClipboardHistory.Commands;
using Microsoft.CmdPal.Extensions.Helpers;
using Windows.ApplicationModel.DataTransfer;

namespace Microsoft.CmdPal.Ext.ClipboardHistory.Models;

public class ClipboardItem
{
    public string Content { get; set; }

    public ClipboardHistoryItem Item { get; set; }

    public DateTimeOffset Timestamp => Item?.Timestamp ?? DateTimeOffset.MinValue;

    public byte[] ImageData { get; set; }

    public string GetDataType()
    {
        // Check if there is valid image data
        if (ImageData != null && ImageData.Length > 0)
        {
            return "Image";
        }

        // Check if there is valid text content
        if (!string.IsNullOrEmpty(Content))
        {
            return "Text";
        }

        return "Unknown";
    }

    private bool IsImage()
    {
        return GetDataType() == "Image";
    }

    private bool IsText()
    {
        return GetDataType() == "Text";
    }

    public ListItem ToListItem()
    {
        ListItem listItem;

        if (IsImage())
        {
            listItem = new(new NoOpCommand())
            {
                // Placeholder subtitle as there’s no BitmapImage dimensions to retrieve
                Title = "Image Data",
                Tags = [new Tag()
                {
                    Text = GetDataType(),
                }
                ],
                Details = new Details { HeroImage = new("\uF0E3"), Title = GetDataType(), Body = Timestamp.ToString(CultureInfo.InvariantCulture) },
            };
        }
        else if (IsText())
        {
            listItem = new(new CopyCommand(this))
            {
                Title = Content,
                Subtitle = GetDataType(),
                Tags = [new Tag()
                {
                    Text = GetDataType(),
                }
                ],
                Details = new Details { Title = GetDataType(), Body = Content },
                MoreCommands = [
                    new CommandContextItem(new PasteCommand(this)),
                ],
            };
        }
        else
        {
            listItem = new(new NoOpCommand())
            {
                Title = "Unknown",
                Subtitle = GetDataType(),
                Tags = [new Tag()
                {
                    Text = GetDataType(),
                }
                ],
                Details = new Details { Title = GetDataType(), Body = Content },
            };
        }

        return listItem;
    }
}
