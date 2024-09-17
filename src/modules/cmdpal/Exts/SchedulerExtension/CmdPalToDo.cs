// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Graph.Models;

namespace SchedulerExtension;

public sealed class CmdPalToDo
{
    public string Title { get; init; } = string.Empty;

    public TaskStatus? Status { get; init; }

    public DateTimeTimeZone DueDateTime { get; init; }

    public DateTimeTimeZone ReminderDateTime { get; init; }

    public PatternedRecurrence Recurrence { get; init; }

    public string Id { get; init; }

    public ItemBody Body { get; init; }
}
