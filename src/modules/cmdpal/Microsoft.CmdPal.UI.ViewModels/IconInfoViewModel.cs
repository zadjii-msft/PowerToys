// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Microsoft.CmdPal.UI.ViewModels.Models;
using Windows.Storage.Streams;

namespace Microsoft.CmdPal.UI.ViewModels;

public partial class IconInfoViewModel : ExtensionObjectViewModel
{
    private readonly ExtensionObject<IconInfo> _model = new(null);

    // These are properties that are "observable" from the extension object
    // itself, in the sense that they get raised by PropChanged events from the
    // extension. However, we don't want to actually make them
    // [ObservableProperty]s, because PropChanged comes in off the UI thread,
    // and ObservableProperty is not smart enough to raise the PropertyChanged
    // on the UI thread.
    public IconDataViewModel Light { get; private set; }

    public IconDataViewModel Dark { get; private set; }

    public IconInfoViewModel(IconInfo? icon, IPageContext errorContext)
        : base(errorContext)
    {
        _model = new(icon);
        Light = new(null, errorContext);
        Dark = new(null, errorContext);
    }

    // Unsafe, needs to be called on BG thread
    public override void InitializeProperties()
    {
        var model = _model.Unsafe;
        if (model == null)
        {
            return;
        }

        Light = new(model.Light, this.PageContext);
        Light.InitializeProperties();

        Dark = new(model.Dark, this.PageContext);
        Dark.InitializeProperties();
    }
}
