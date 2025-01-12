// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Microsoft.CmdPal.UI.ViewModels.Models;
using Windows.Storage.Streams;

namespace Microsoft.CmdPal.UI.ViewModels;

public partial class IconDataViewModel : ExtensionObjectViewModel
{
    private readonly ExtensionObject<IconData> _model = new(null);

    // Locally cached properties from IconData.
    public string Icon { get; private set; } = string.Empty;

    // Streams are not trivially copyable, so we can't copy the data locally
    // first. Hence why we're sticking this into an ExtensionObject
    public ExtensionObject<IRandomAccessStreamReference> Data { get; private set; } = new(null);

    public IconDataViewModel(IconData? icon, IPageContext errorContext)
        : base(errorContext)
    {
        _model = new(icon);
    }

    // Unsafe, needs to be called on BG thread
    public override void InitializeProperties()
    {
        var model = _model.Unsafe;
        if (model == null)
        {
            return;
        }

        Icon = model.Icon;
        Data = new(model.Data);
    }
}
