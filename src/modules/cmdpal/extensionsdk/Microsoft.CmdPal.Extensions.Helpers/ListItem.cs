﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.CmdPal.Extensions.Helpers;

public class ListItem : CommandItem, IListItem
{
    private ITag[] _tags = [];
    private IDetails? _details;

    // private IFallbackHandler? _fallbackHandler;
    private string _section = string.Empty;
    private string _textToSuggest = string.Empty;

    public ITag[] Tags
    {
        get => _tags;
        set
        {
            _tags = value;
            OnPropertyChanged(nameof(Tags));
        }
    }

    public IDetails? Details
    {
        get => _details;
        set
        {
            _details = value;
            OnPropertyChanged(nameof(Details));
        }
    }

    // public IFallbackHandler? FallbackHandler
    // {
    //    get => _fallbackHandler ?? _command as IFallbackHandler;
    //    init => _fallbackHandler = value;
    // }
    public string Section
    {
        get => _section;
        set
        {
            _section = value;
            OnPropertyChanged(nameof(Section));
        }
    }

    public string TextToSuggest
    {
        get => _textToSuggest;
        set
        {
            _textToSuggest = value;
            OnPropertyChanged(nameof(TextToSuggest));
        }
    }

    public ListItem(ICommand command)
        : base(command)
    {
    }

    public ListItem(ICommandItem command)
        : base(command)
    {
    }
}
