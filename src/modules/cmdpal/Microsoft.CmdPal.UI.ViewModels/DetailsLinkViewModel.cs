// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.UI.ViewModels.Models;

namespace Microsoft.CmdPal.UI.ViewModels;

public partial class DetailsLinkViewModel(IDetailsLink _detailsData, IPageContext context) : DetailsDataViewModel(context)
{
    private readonly ExtensionObject<IDetailsLink> _model = new(_detailsData);

    public string Text { get; private set; } = string.Empty;
    public Uri? Link { get; private set; };

    public override void InitializeProperties()
    {
        var model = _model.Unsafe;
        if (model == null)
        {
            return;
        }

        Text = model.Text ?? string.Empty;
        Link = model.Link;
        UpdateProperty(nameof(Text));
        UpdateProperty(nameof(Link));
    }
}
