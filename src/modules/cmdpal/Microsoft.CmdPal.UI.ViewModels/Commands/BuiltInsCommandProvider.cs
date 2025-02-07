// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.CmdPal.Common;
using Microsoft.CmdPal.UI.ViewModels.Messages;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.Foundation;

namespace Microsoft.CmdPal.UI.ViewModels.BuiltinCommands;

/// <summary>
/// Built-in Provider for a top-level command which can quit the application. Invokes the <see cref="QuitCommand"/>, which sends a <see cref="QuitMessage"/>.
/// </summary>
public partial class BuiltInsCommandProvider : CommandProvider
{
    private readonly OpenSettingsCommand openSettings = new();
    private readonly QuitCommand quitCommand = new();
    private readonly FallbackReloadItem _fallbackReloadItem = new();
    private readonly FallbackLogItem _fallbackLogItem = new();
    private readonly NewExtensionPage _newExtension = new();

    public override ICommandItem[] TopLevelCommands() =>
        [
            new CommandItem(openSettings) { Subtitle = "Open Command Palette settings" },
            new CommandItem(_newExtension) { Title = _newExtension.Title, Subtitle = "Creates a project for a new Command Palette extension" },
        ];

    public override IFallbackCommandItem[] FallbackCommands() =>
        [
            new FallbackCommandItem(quitCommand) { Subtitle = "Exit Command Palette" },
            _fallbackReloadItem,
            _fallbackLogItem,
        ];

    public BuiltInsCommandProvider()
    {
        Id = "Core";
        DisplayName = "Built-in commands";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.scale-200.png");
    }

    public override void InitializeWithHost(IExtensionHost host) => BuiltinsExtensionHost.Instance.Initialize(host);
}

[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "testing")]
public partial class BuiltinsExtensionHost
{
    internal static ExtensionHostInstance Instance { get; } = new();
}

[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "testing")]
public partial class NewExtensionPage : ContentPage
{
    private readonly NewExtensionForm _inputForm = new();
    private IFormContent? _resultForm;

    public override IContent[] GetContent()
    {
        if (_resultForm != null)
        {
            return [_resultForm];
        }

        return [_inputForm];
    }

    public NewExtensionPage()
    {
        Name = "Open";
        Title = "Create a new extension";
        Icon = new IconInfo("\uEA86"); // Puzzle

        _inputForm.FormSubmitted += FormSubmitted;
    }

    private void FormSubmitted(NewExtensionForm sender, IFormContent args)
    {
        _resultForm = args;
        RaiseItemsChanged(1);
    }
}

[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "testing")]
internal sealed partial class NewExtensionForm : FormContent
{
    private static readonly string _creatingText = "Creating new extension...";
    private readonly StatusMessage _creatingMessage = new()
    {
        Message = _creatingText,
        Progress = new ProgressState() { IsIndeterminate = true },
    };

    public event TypedEventHandler<NewExtensionForm, IFormContent>? FormSubmitted;

    public NewExtensionForm()
    {
        TemplateJson = $$"""
{
    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
    "type": "AdaptiveCard",
    "version": "1.6",
    "body": [
        {
            "type": "TextBlock",
            "text": "Create your new extension",
            "size": "large"
        },
        {
            "type": "TextBlock",
            "text": "Use this page to create a new extension project.",
            "wrap": true
        },
        {
            "type": "TextBlock",
            "text": "Extension name",
            "weight": "bolder",
            "size": "default"
        },
        {
            "type": "TextBlock",
            "text": "This is the name of your new extension project. It should be a valid C# class name. Best practice is to also include the word 'Extension' in the name.",
            "wrap": true
        },
        {
            "type": "Input.Text",
            "label": "Extension name",
            "isRequired": true,
            "errorMessage": "Extension name is required, without spaces",
            "id": "ExtensionName",
            "placeholder": "ExtensionName",
            "regex": "^[^\\s]+$"
        },
        {
            "type": "TextBlock",
            "text": "Display name",
            "weight": "bolder",
            "size": "default"
        },
        {
            "type": "TextBlock",
            "text": "The name of your extension as users will see it.",
            "wrap": true
        },
        {
            "type": "Input.Text",
            "label": "Display name",
            "isRequired": true,
            "errorMessage": "Display name is required",
            "id": "DisplayName",
            "placeholder": "My new extension"
        },
        {
            "type": "TextBlock",
            "text": "Output path",
            "weight": "bolder",
            "size": "default"
        },
        {
            "type": "TextBlock",
            "text": "Where should the new extension be created? This path will be created if it doesn't exist",
            "wrap": true
        },
        {
            "type": "Input.Text",
            "label": "Output path",
            "isRequired": true,
            "errorMessage": "Output path is required",
            "id": "OutputPath",
            "placeholder": "C:\\users\\me\\dev"
        }
    ],
    "actions": [
        {
            "type": "Action.Submit",
            "title": "Create extension",
            "associatedInputs": "auto"
        }
    ]
}
""";
    }

    public override CommandResult SubmitForm(string payload)
    {
        var formInput = JsonNode.Parse(payload)?.AsObject();
        if (formInput == null)
        {
            return CommandResult.KeepOpen();
        }

        var extensionName = formInput["ExtensionName"]?.AsValue()?.ToString() ?? string.Empty;
        var displayName = formInput["DisplayName"]?.AsValue()?.ToString() ?? string.Empty;
        var outputPath = formInput["OutputPath"]?.AsValue()?.ToString() ?? string.Empty;

        _creatingMessage.State = MessageState.Info;
        _creatingMessage.Message = _creatingText;
        _creatingMessage.Progress = new ProgressState() { IsIndeterminate = true };
        BuiltinsExtensionHost.Instance.ShowStatus(_creatingMessage);

        try
        {
            CreateExtension(extensionName, displayName, outputPath);

            // _creatingMessage.Progress = null;
            // _creatingMessage.State = MessageState.Success;
            // _creatingMessage.Message = $"Successfully created extension";
            BuiltinsExtensionHost.Instance.HideStatus(_creatingMessage);

            // BuiltinsExtensionHost.Instance.HideStatus(_creatingMessage);
            FormSubmitted?.Invoke(this, new CreatedExtensionForm(extensionName, displayName, outputPath));

            // _toast.Message.State = MessageState.Success;
            // _toast.Message.Message = $"Successfully created extension";
            // _toast.Show();
        }
        catch (Exception e)
        {
            BuiltinsExtensionHost.Instance.HideStatus(_creatingMessage);

            _creatingMessage.State = MessageState.Error;
            _creatingMessage.Message = $"Error: {e.Message}";

            // _toast.Show();
        }

        // _ = Task.Run(() =>
        // {
        //    Thread.Sleep(2500);
        //    BuiltinsExtensionHost.Instance.HideStatus(_creatingMessage);
        // });
        return CommandResult.KeepOpen();
    }

    private void CreateExtension(string extensionName, string newDisplayName, string outputPath)
    {
        var newGuid = Guid.NewGuid().ToString();

        // Unzip `template.zip` to a temp dir:
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        // Console.WriteLine($"Extracting to {tempDir}");

        // Does the output path exist?
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        var assetsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory.ToString(), "Assets\\template.zip");
        ZipFile.ExtractToDirectory(assetsPath, tempDir);

        var files = Directory.GetFiles(tempDir, "*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var text = File.ReadAllText(file);

            Console.WriteLine($"  Processing {file}");

            // Replace all the instances of `FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF` with a new random guid:
            text = text.Replace("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF", newGuid);

            // Then replace all the `TemplateCmdPalExtension` with `extensionName`
            text = text.Replace("TemplateCmdPalExtension", extensionName);

            // Then replace all the `TemplateDisplayName` with `newDisplayName`
            text = text.Replace("TemplateDisplayName", newDisplayName);

            // We're going to write the file to the same relative location in the output path
            var relativePath = Path.GetRelativePath(tempDir, file);

            var newFileName = Path.Combine(outputPath, relativePath);

            // if the file name had `TemplateCmdPalExtension` in it, replace it with `extensionName`
            newFileName = newFileName.Replace("TemplateCmdPalExtension", extensionName);

            // Make sure the directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(newFileName)!);

            File.WriteAllText(newFileName, text);

            Console.WriteLine($"  Wrote {newFileName}");

            // Delete the old file
            File.Delete(file);
        }

        // Delete the temp dir
        Directory.Delete(tempDir, true);
    }
}

[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "testing")]
public partial class CreatedExtensionForm : FormContent
{
    public CreatedExtensionForm(string name, string displayName, string path)
    {
        TemplateJson = CardTemplate;
        DataJson = $$"""
{
    "name": {{JsonSerializer.Serialize(name)}},
    "directory": {{JsonSerializer.Serialize(path)}},
    "displayName": {{JsonSerializer.Serialize(displayName)}}
}
""";
    }

    private static readonly string CardTemplate = """
{
    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
    "type": "AdaptiveCard",
    "version": "1.6",
    "body": [
        {
            "type": "TextBlock",
            "text": "Successfully created your new extension!",
            "size": "large",
            "weight": "bolder",
            "style": "heading",
            "wrap": true
        },
        {
            "type": "TextBlock",
            "text": "Your new extension \"${displayName}\" was created in:",
            "wrap": true
        },
        {
            "type": "TextBlock",
            "text": "${directory}",
            "fontType": "Monospace"
        },
        {
            "type": "TextBlock",
            "text": "Next steps",
            "style": "heading",
            "wrap": true
        },
        {
            "type": "TextBlock",
            "text": "Now that your extension project has been created, open the solution up in Visual Studio to start writing your extension code.",
            "wrap": true
        },
        {
            "type": "TextBlock",
            "text": "Navigate to ${name}Page.cs to start adding items to the list, or to ${name}CommandsProvider.cs to add new commands.",
            "wrap": true
        },
        {
            "type": "TextBlock",
            "text": "Once you're ready to test deploy the package locally with Visual Studio, then run the \"Reload\" command in the Command Palette to load your new extension.",
            "wrap": true
        }
    ],
    "actions": [
        {
            "type": "Action.Submit",
            "title": "Open Solution",
            "data": {
                "x": "sln"
            }
        },
        {
            "type": "Action.Submit",
            "title": "Open directory",
            "data": {
                "x": "dir"
            }
        },
        {
            "type": "Action.Submit",
            "title": "Create another",
            "data": {
                "x": "new"
            }
        }
    ]
}
""";
}
