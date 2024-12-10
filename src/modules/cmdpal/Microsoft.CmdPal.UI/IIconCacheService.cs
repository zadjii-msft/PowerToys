﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Extensions;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.CmdPal.UI;

public interface IIconCacheService
{
    public Task<IconSource?> GetIconSource(IconDataType icon);
}
