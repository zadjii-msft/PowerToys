// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.CmdPal.UI.Controls;

internal sealed partial class AmbLight : XamlLight
{
    private static readonly string Id = typeof(AmbLight).FullName!;

    protected override void OnConnected(UIElement newElement)
    {
        var compositor = CompositionTarget.GetCompositorForCurrentThread();

        // Create AmbientLight and set its properties
        var ambientLight = compositor.CreateAmbientLight();
        ambientLight.Color = Colors.White;

        // Associate CompositionLight with XamlLight
        CompositionLight = ambientLight;

        // Add UIElement to the Light's Targets
        AddTargetElement(GetId(), newElement);
    }

    protected override void OnDisconnected(UIElement oldElement)
    {
        // Dispose Light when it is removed from the tree
        RemoveTargetElement(GetId(), oldElement);
        CompositionLight.Dispose();
    }

    protected override string GetId() => Id;
}
