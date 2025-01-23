// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.UI.ViewModels.Models;

namespace Microsoft.CmdPal.UI.ViewModels;

public partial class DetailsElementViewModel(IDetailsElement _detailsElement, IPageContext context) : ExtensionObjectViewModel(context)
{
    private readonly ExtensionObject<IDetailsElement> _model = new(_detailsElement);

    public string Key { get; private set; } = string.Empty;

    public DetailsDataViewModel? Data { get; private set; }

    public override void InitializeProperties()
    {
        var model = _model.Unsafe;
        if (model == null)
        {
            return;
        }

        Key = model.Key ?? string.Empty;
        UpdateProperty(nameof(Key));

        var data = model.Data;
        if (data != null)
        {
            DetailsDataViewModel? vm = data switch
            {
                IDetailsSeparator => new DetailsSeparatorViewModel((IDetailsSeparator)data, this.PageContext),
                IDetailsLink => new DetailsLinkViewModel((IDetailsLink)data, this.PageContext),
                IDetailsTags => new DetailsTagsViewModel((IDetailsTags)data, this.PageContext),
                _ => null,
            };
            if (vm != null)
            {
                vm.InitializeProperties();
                Data = vm;
                UpdateProperty(nameof(Data));
            }
        }
    }
}
