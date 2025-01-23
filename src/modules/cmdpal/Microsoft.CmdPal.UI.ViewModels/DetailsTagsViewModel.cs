// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.UI.ViewModels.Models;

namespace Microsoft.CmdPal.UI.ViewModels;

public partial class DetailsTagsViewModel(IDetailsTags _detailsData, IPageContext context) : DetailsDataViewModel(context)
{
    private readonly ExtensionObject<IDetailsTags> _model = new(_detailsData);

    public override void InitializeProperties()
    {
        var model = _model.Unsafe;
        if (model == null)
        {
            return;
        }

        // TODO!
    }
}
