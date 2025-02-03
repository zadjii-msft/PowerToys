// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Serialization;
using static Microsoft.CmdPal.Extensions.Helpers.ChoiceSetSetting;

namespace Microsoft.CmdPal.Extensions.Helpers;

[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(Choice))]
[JsonSerializable(typeof(List<Choice>))]
[JsonSerializable(typeof(List<ChoiceSetSetting>))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(float))]
[JsonSerializable(typeof(Dictionary<string, object>), TypeInfoPropertyName = "Dictionary")]
[JsonSourceGenerationOptions(UseStringEnumConverter = true, WriteIndented = true)]
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Just used here")]
public partial class JsonSerializationContext : JsonSerializerContext
{
}
