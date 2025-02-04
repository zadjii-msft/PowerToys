﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.Foundation;

namespace Microsoft.CmdPal.Ext.Calc;

public partial class CalculatorCommandProvider : CommandProvider
{
    // private readonly CalculatorTopLevelListItem calculatorCommand = new();
    private readonly ListItem _listItem = new(new CalculatorListPage()) { Subtitle = "Press = to type an equation" };

    public CalculatorCommandProvider()
    {
        Id = "Calculator";
        DisplayName = "Calculator";
        Icon = new("\ue8ef"); // Calculator
    }

    public override ICommandItem[] TopLevelCommands() => [_listItem];
}

// todo
// list page, dynamic
// first SaveCommand, title=result, subtitle=query, more:copy to clipboard
//  - when you save, insert into list at spot 1
//  - also on save, change searchtext to result
// rest:
//  * copy, suggest result
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This is sample code")]
public sealed partial class CalculatorListPage : DynamicListPage
{
    private readonly List<ListItem> _items = [];
    private readonly SaveCommand _saveCommand = new();

    public CalculatorListPage()
    {
        Icon = new("\ue8ef"); // Calculator
        Name = "Calculator";
        PlaceholderText = "Type an equation...";
        Id = "com.microsoft.cmdpal.calculator";

        _items.Add(new(_saveCommand)
        {
            MoreCommands = [new CommandContextItem(new CopyTextCommand(string.Empty))],
        });

        UpdateSearchText(string.Empty, string.Empty);

        _saveCommand.SaveRequested += HandleSave;
    }

    private void HandleSave(object sender, object args)
    {
        var lastResult = _items[0].Title;
        if (!string.IsNullOrEmpty(lastResult))
        {
            var li = new ListItem(new CopyTextCommand(lastResult))
            {
                Title = _items[0].Title,
                Subtitle = _items[0].Subtitle,
                TextToSuggest = lastResult,
            };
            _items.Insert(1, li);
            _items[0].Subtitle = string.Empty;
            SearchText = lastResult;
            this.RaiseItemsChanged(this._items.Count);
        }
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        if (string.IsNullOrEmpty(newSearch))
        {
            _items[0].Title = "Type an equation...";
            _items[0].Subtitle = string.Empty;
        }
        else
        {
            _items[0].TextToSuggest = ParseQuery(newSearch, out var result) ? result : string.Empty;
            _items[0].Title = result;
            _items[0].Subtitle = newSearch;
        }
    }

    private bool ParseQuery(string equation, out string result)
    {
        try
        {
            var resultNumber = new DataTable().Compute(equation, null);
            result = resultNumber.ToString() ?? string.Empty;
            return true;
        }
        catch (Exception e)
        {
            result = $"Error: {e.Message}";
            return false;
        }
    }

    public override IListItem[] GetItems() => _items.ToArray();
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This is sample code")]
public sealed partial class SaveCommand : InvokableCommand
{
    public event TypedEventHandler<object, object> SaveRequested;

    public SaveCommand()
    {
        Name = "Save";
    }

    public override ICommandResult Invoke()
    {
        SaveRequested?.Invoke(this, this);
        return CommandResult.KeepOpen();
    }
}
