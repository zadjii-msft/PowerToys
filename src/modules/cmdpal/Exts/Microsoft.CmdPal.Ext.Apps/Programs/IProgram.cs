﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.CmdPal.Ext.Apps.Programs;

public interface IProgram
{
    // List<ContextMenuResult> ContextMenus(string queryArguments, IPublicAPI api);
    // Result Result(string query, string queryArguments, IPublicAPI api);
    string UniqueIdentifier { get; set; }

    string Name { get; }

    string Description { get; set; }

    string Location { get; }

    bool Enabled { get; set; }
}