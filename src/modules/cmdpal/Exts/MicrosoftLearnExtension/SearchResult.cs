// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Serialization;

namespace MicrosoftLearnExtension;

// We don't care about members not listed below - https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/missing-members
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Skip)]
public class SearchResult
{
    public string Title { get; set; }

    public string Url { get; set; }

    public string Description { get; set; }
}
