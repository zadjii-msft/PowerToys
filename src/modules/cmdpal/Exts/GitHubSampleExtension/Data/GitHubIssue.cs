// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GitHubSampleExtension.Data;

public sealed class GitHubIssue
{
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; }

    [JsonPropertyName("body")]
    public string Body { get; set; }
}
