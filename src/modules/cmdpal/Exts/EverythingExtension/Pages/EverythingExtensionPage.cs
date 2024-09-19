// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
        Name = "Everything";

        Everything_SetRequestFlags(Request.FILE_NAME | Request.PATH);
        Everything_SetSort(Sort.NAME_ASCENDING);
        Everything_SetMax(50);
    }

    public override ISection[] GetItems(string query)
    {
        Everything_SetSearchW(query);

        if (!Everything_QueryW(true))
        {
            throw new Win32Exception("Unable to Query");
        }

        var resultCount = Everything_GetNumResults();

        // Create a new ListSections
        var section = new ListSection();

        // Create a List to store ListItems
        var itemList = new List<ListItem>();

        // Loop through the results and add them to the List
        for (uint i = 0; i < resultCount; i++)
        {
            // Get the result file name
            var fileName = Marshal.PtrToStringUni(Everything_GetResultFileNameW(i));

            // Get the result file path
            var filePath = Marshal.PtrToStringUni(Everything_GetResultPathW(i));

            // Concatenate the file path and file name
            var fullTitle = Path.Combine(filePath, fileName);

            // System.Drawing.Icon ic = System.Drawing.Icon.ExtractAssociatedIcon(fullTitle);
            itemList.Add(new ListItem(new OpenFileCommand(fullTitle, filePath)) { Title = fileName, Subtitle = filePath });
        }

        // Convert the List to an array and assign it to the Items property
        section.Items = itemList.ToArray();

        // Return the ListSection with the items
        return [section];
    }
}
