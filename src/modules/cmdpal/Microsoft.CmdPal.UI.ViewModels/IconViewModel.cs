// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Extensions;
using Windows.Storage.Streams;

namespace Microsoft.CmdPal.UI.ViewModels;

public sealed class IconViewModel(IconDataType? _icon)
{
    public string Icon { get; } = _icon?.Icon ?? string.Empty;

    public IRandomAccessStreamReference? Data { get; } = _icon?.Data;

    public bool HasIcon => !string.IsNullOrWhiteSpace(Icon) || Data != null;

    public override string ToString() => string.IsNullOrEmpty(Icon) ? "[Binary data]" : Icon;
}
