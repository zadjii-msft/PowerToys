// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.UI.ViewModels.Messages;
using Microsoft.CmdPal.UI.ViewModels.Models;

namespace Microsoft.CmdPal.UI.ViewModels;

public abstract partial class ExtensionObjectViewModel : ObservableObject
{
    public async virtual Task InitializePropertiesAsync()
    {
        var t = new Task(() =>
        {
            try
            {
                Initialize();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        });
        t.Start();
        await t;
    }

    protected abstract void Initialize();
}
