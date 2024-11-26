// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.UI.ViewModels.Models;

namespace Microsoft.CmdPal.UI.ViewModels;

public partial class ListItemViewModel : CommandItemViewModel
{
    public ExtensionObject<IListItem> Model { get; }

    public ITag[] Tags { get; private set; } = [];

    public bool HasTags => Tags.Length > 0;

    public ListItemViewModel(IListItem model)
        : base(new(model))
    {
        Model = new(model);
    }

    public ListItemViewModel(ExtensionObject<IListItem> model)
        : base(new ExtensionObject<ICommandItem>(model.Unsafe))
    {
        Model = model;
    }

    protected override void Initialize()
    {
        base.Initialize();

        // TODO load tags here, details, suggested text, all that
        var li = Model.Unsafe;
        if (li == null)
        {
            return; // throw?
        }

        // TODO TagViewModel not ITag
        Tags = li.Tags ?? [];
    }
}
