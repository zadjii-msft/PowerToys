// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using static EverythingExtension.NativeMethods;

namespace EverythingExtension;

internal sealed partial class EverythingExtensionPage : DynamicListPage
{
    public EverythingExtensionPage()
    {
        Icon = new(string.Empty);
        Name = "Everything extension for cmdpal";

        Everything_SetRequestFlags(Request.FILE_NAME | Request.PATH);
        Everything_SetSort(Sort.PATH_ASCENDING);
    }

    public override ISection[] GetItems(string query)
    {
        Everything_SetSearchW(query);

        if (!Everything_QueryW(true))
        {
            throw new Win32Exception("Unable to Query");
        }

        var resultCount = Everything_GetNumResults();

        Console.WriteLine(resultCount);

        ListItem[] items = new ListItem[resultCount];

        // Loop through the results and add them to the list
        for (uint i = 0; i < resultCount; i++)
        {
            // Get the result file name (or full path) from Everything
            var result = Marshal.PtrToStringUni(Everything_GetResultFileNameW(i));

            // Create a new ListItem for each result and set its title
            items[i] = new ListItem(new NoOpCommand()) { Title = result };
        }

        // Return the ListSection with the items
        return [
            new ListSection()
            {
                    Title = "Results",
                    Items = items,
            },
        ];
    }
}
