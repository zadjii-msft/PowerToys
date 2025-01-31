// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Windows.Foundation;

namespace Microsoft.CmdPal.Extensions.Helpers;

public abstract partial class FormContent : BaseObservable, IFormContent
{
    public virtual string DataJson
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(DataJson));
        }
    }

= string.Empty;

    public virtual string StateJson
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(StateJson));
        }
    }

= string.Empty;

    public virtual string TemplateJson
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(TemplateJson));
        }
    }

= string.Empty;

    public abstract ICommandResult SubmitForm(string payload);
}
