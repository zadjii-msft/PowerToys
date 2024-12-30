// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Extensions;

namespace Microsoft.CmdPal.UI.ViewModels;

public interface IPageViewModel
{
    public bool IsNested { get; set; }

    public string Filter { get; set; }

    public string Title { get; }

    public bool IsLoading { get; }

    public IconDataType Icon { get; }
}
