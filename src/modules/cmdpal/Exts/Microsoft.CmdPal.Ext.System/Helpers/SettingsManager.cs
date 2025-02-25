// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace Microsoft.CmdPal.Ext.System.Helpers;

public class SettingsManager : JsonSettingsManager
{
    private static readonly string _namespace = "system";

    private static string Namespaced(string propertyName) => $"{_namespace}.{propertyName}";

    private readonly ToggleSetting _includeInGlobalResult = new(
        Namespaced(nameof(IncludeInGlobalResult)),
        "Include in global result",
        "Include in global result",
        true); // TODO -- double check default value

    private readonly ToggleSetting _showDialogToConfirmCommand = new(
        Namespaced(nameof(ShowDialogToConfirmCommand)),
        "Show a dialog to confirm system commands",
        "Show a dialog to confirm system commands",
        false); // TODO -- double check default value

    private readonly ToggleSetting _showSuccessMessageAfterEmptyingRecycleBin = new(
        Namespaced(nameof(ShowSuccessMessageAfterEmptyingRecycleBin)),
        "Show a success message after emptying the Recycle Bin",
        "Show a success message after emptying the Recycle Bin",
        false); // TODO -- double check default value

    private readonly ToggleSetting _useLocalizedSystemCommandsInsteadOfEnglishOnes = new(
        Namespaced(nameof(UseLocalizedSystemCommandsInsteadOfEnglishOnes)),
        "Use localized system commands instead of English ones",
        "Use localized system commands instead of English ones",
        true); // TODO -- double check default value

    private readonly ToggleSetting _showSeparateResultForEmptyRecycleBin = new(
        Namespaced(nameof(ShowSeparateResultForEmptyRecycleBin)),
        "Show separate result for Empty Recycle Bin command",
        "Show separate result for Empty Recycle Bin command",
        true); // TODO -- double check default value

    private readonly ToggleSetting _reduceThePriorityOfIPAndMacResults = new(
        Namespaced(nameof(ReduceThePriorityOfIPAndMacResults)),
        "Reduce the priority of 'IP' and 'MAC' results to improve the order in the global results",
        "Reduce the priority of 'IP' and 'MAC' results to improve the order in the global results",
        true); // TODO -- double check default value

    public bool IncludeInGlobalResult => _includeInGlobalResult.Value;

    public bool ShowDialogToConfirmCommand => _showDialogToConfirmCommand.Value;

    public bool ShowSuccessMessageAfterEmptyingRecycleBin => _showSuccessMessageAfterEmptyingRecycleBin.Value;

    public bool UseLocalizedSystemCommandsInsteadOfEnglishOnes => _useLocalizedSystemCommandsInsteadOfEnglishOnes.Value;

    public bool ShowSeparateResultForEmptyRecycleBin => _showSeparateResultForEmptyRecycleBin.Value;

    public bool ReduceThePriorityOfIPAndMacResults => _reduceThePriorityOfIPAndMacResults.Value;

    internal static string SettingsJsonPath()
    {
        var directory = Utilities.BaseSettingsPath("Microsoft.CmdPal");
        Directory.CreateDirectory(directory);

        // now, the state is just next to the exe
        return Path.Combine(directory, "settings.json");
    }

    public SettingsManager()
    {
        FilePath = SettingsJsonPath();

        Settings.Add(_includeInGlobalResult);
        Settings.Add(_showDialogToConfirmCommand);
        Settings.Add(_showSuccessMessageAfterEmptyingRecycleBin);
        Settings.Add(_useLocalizedSystemCommandsInsteadOfEnglishOnes);
        Settings.Add(_showSeparateResultForEmptyRecycleBin);
        Settings.Add(_reduceThePriorityOfIPAndMacResults);

        // Load settings from file upon initialization
        LoadSettings();
    }
}
