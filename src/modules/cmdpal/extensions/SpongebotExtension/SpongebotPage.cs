// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Windows.CommandPalette.Extensions;
using Microsoft.Windows.CommandPalette.Extensions.Helpers;

namespace SpongebotExtension;

public class SpongebotPage : MarkdownPage, IFallbackHandler
{
    internal CopyTextAction CopyTextAction { get; set; }

    // Name, Icon, IPropertyChanged: all those are defined in the MarkdownPage base class
    public SpongebotPage()
    {
        Name = string.Empty;
        Icon = new("https://imgflip.com/s/meme/Mocking-Spongebob.jpg");
        Commands = [
            new CommandContextItem(CopyTextAction)
        ];
    }

    public void UpdateQuery(string query)
    {
        if (string.IsNullOrEmpty(query))
        {
            Name = string.Empty;
        }
        else
        {
            Name = ConvertToAlternatingCase(query);
        }

        CopyTextAction.Text = Name;
    }

    private static string ConvertToAlternatingCase(string input)
    {
        StringBuilder sb = new StringBuilder();
        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
#pragma warning disable CA1304 // Specify CultureInfo
            sb.Append(i % 2 == 0 ? char.ToUpper(c) : char.ToLower(c));
#pragma warning restore CA1304 // Specify CultureInfo
        }

        return sb.ToString();
    }

    public override string[] Bodies()
    {
        var t = GenerateMeme(this.Name);
        t.ConfigureAwait(false);

        return [t.Result];
    }

    private static async Task<string> GenerateMeme(string text)
    {
        var client = new System.Net.Http.HttpClient();

        var state = File.ReadAllText(StateJsonPath());
        var jsonState = JsonNode.Parse(state);

        var bodyObj = new Dictionary<string, string>
        {
            { "template_id", "102156234" },
            { "username", jsonState["username"]?.ToString() ?? string.Empty },
            { "password", jsonState["password"]?.ToString() ?? string.Empty },
            { "boxes[0][x]", "0" },
            { "boxes[0][y]", "289" },
            { "boxes[0][width]", "502" },
            { "boxes[0][height]", "49" },
            { "boxes[0][text]", text },
        };
        var content = new System.Net.Http.FormUrlEncodedContent(bodyObj);
        var resp = await client.PostAsync("https://api.imgflip.com/caption_image", content);
        var respBody = await resp.Content.ReadAsStringAsync();
        var response = JsonNode.Parse(respBody);

        var url = response["data"]?["url"]?.ToString() ?? string.Empty;

        var body = $$"""
 SpongeBot says:
![{{text}}]({{url}})

[{{text}}]({{url}})
""";
        return body;
    }

    internal static string StateJsonPath()
    {
        // Get the path to our exe
        var path = System.Reflection.Assembly.GetExecutingAssembly().Location;

        // Get the directory of the exe
        var directory = Path.GetDirectoryName(path) ?? string.Empty;

        // now, the state is just next to the exe
        return Path.Combine(directory, "state.json");
    }
}

/*

// internal sealed class SettingsPage : IPage
// {
//     public IAsyncOperation<string> RenderToJson()
//     {
//         var json = $$"""
// {
//   "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
//   "type": "AdaptiveCard",
//   "version": "1.5",
//   "body": [
//     {
//       "type": "Input.Text",
//       "style": "text",
//       "id": "username",
//       "label": "Username",
//       "isRequired": true,
//       "errorMessage": "Username is required"
//     },
//     {
//       "type": "Input.Text",
//       "style": "password",
//       "id": "password",
//       "label": "Password",
//       "isRequired": true,
//       "errorMessage": "Password is required"
//     }
//   ],
//   "actions": [
//     {
//       "type": "Action.Submit",
//       "title": "Save",
//       "data": {
//         "username": "username",
//         "password": "password"
//       }
//     }
//   ]
// }
// """;

//         return Task.FromResult(json).AsAsyncOperation();
//     }
// }

// public sealed class SettingsCommand : ICommand
// {
//     private readonly IPage _page = new SettingsPage();
//     public CommandType Kind => CommandType.Form;
//     public string Icon => "";

//     public string Name => "Spongebob settings";
//     public string Subtitle => "You need to add your imgflip account info here";
//     public IPage Page => _page;
//     public IAsyncOperation<IReadOnlyList<ICommand>> GetCommandsForQueryAsync(string search) { return null; }

//     public event TypedEventHandler<object, object> RequestRefresh;

// #pragma warning disable CS0067
//     public event TypedEventHandler<object, NavigateToCommandRequestedEventArgs> NavigateToCommandRequested;
// #pragma warning restore

//     public IAsyncAction DoAction(string actionId)
//     {
//         var formInput = JObject.Parse(actionId);

//         // get the name and url out of the values
//         var formName = formInput["username"];
//         var formPassword = formInput["password"];

//         // Construct a new json blob with the name and url
//         var json =  $$"""
// {
//     "username": "{{formName}}",
//     "password": "{{formPassword}}"
// }
// """;

//         File.WriteAllText(SettingsCommand.StateJsonPath(), json);

//         var handlers = RequestRefresh;
//         handlers?.Invoke(this, new());

//         return Task.CompletedTask.AsAsyncAction();
//     }

//     internal static string StateJsonPath()
//     {
//         // Get the path to our exe
//         var path = System.Reflection.Assembly.GetExecutingAssembly().Location;
//         // Get the directory of the exe
//         var directory = System.IO.Path.GetDirectoryName(path);
//         // now, the state is just next to the exe
//         return System.IO.Path.Combine(directory, "state.json");
//     }
// }
*/
