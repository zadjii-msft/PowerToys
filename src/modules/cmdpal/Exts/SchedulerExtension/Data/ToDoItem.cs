// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SchedulerExtension.Data;

public sealed class ToDoItem
{
    [JsonPropertyName("importance")]
    public string Importance { get; set; }

    [JsonPropertyName("isReminderOn")]
    public bool IsReminderOn { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("createdDateTime")]
    public DateTime CreatedDateTime { get; set; }

    [JsonPropertyName("lastModifiedDateTime")]
    public DateTime LastModifiedDateTime { get; set; }

    [JsonPropertyName("hasAttachments")]
    public bool HasAttachments { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("completedDateTime")]
    public CompletedDateTime CompletedDateTime { get; set; }

    public string ListId { get; set; }
}
