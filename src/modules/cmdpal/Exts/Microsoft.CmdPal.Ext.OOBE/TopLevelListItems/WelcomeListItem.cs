// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Data;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace Microsoft.CmdPal.Ext.OOBE.TopLevelListItems;

public partial class WelcomeListItem : ListItem
{
    public WelcomeListItem()
        : base(new WelcomeListPage())
    {
        Icon = new("\uF133");
        Title = "Welcome to the Command Palette";
        Subtitle = "3/12 complete";
        Section = "Welcome Jordi";
        Tags = [new Tag()
        {
            Text = "Tutorial",
        }
        ];
    }
}
