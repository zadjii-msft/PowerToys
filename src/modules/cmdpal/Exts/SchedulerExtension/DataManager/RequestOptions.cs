﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Threading;

namespace SchedulerExtension;

public class RequestOptions
{
    public List<string> Fields { get; set; }

    public CancellationToken? CancellationToken { get; set; }

    public bool Refresh { get; set; }

    public RequestOptions()
    {
        Fields = new List<string>();
    }

    public static RequestOptions RequestOptionsDefault()
    {
        // Default options are a limited fetch of 10 items intended for widget display.
        var defaultOptions = new RequestOptions
        {
            Fields = new List<string>
            {
                // "System.Id" is implicitly added.
                "System.State",
                "System.Reason",
                "System.AssignedTo",
                "System.CreatedDate",
                "System.CreatedBy",
                "System.ChangedDate",
                "System.ChangedBy",
                "System.Title",
                "System.WorkItemType",
            },
        };

        return defaultOptions;
    }

    public override string ToString()
    {
        return $"Request Fields: {string.Join(",", Fields.ToArray())}";
    }
}
