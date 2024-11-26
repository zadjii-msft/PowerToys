// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using Microsoft.CmdPal.Ext.WebSearch.Pages;
using Microsoft.CmdPal.Ext.WebSearch.Properties;
using Microsoft.CmdPal.Extensions.Helpers;

namespace Microsoft.CmdPal.Ext.WebSearch;

public partial class WebSearchTopLevelCommandItem : CommandItem
{
    public WebSearchTopLevelCommandItem()
        : base(new WebSearchListPage())
    {
        Icon = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory.ToString(), "Images\\WebSearch.dark.png"));
        Title = Resources.command_item_title;
    }
}
