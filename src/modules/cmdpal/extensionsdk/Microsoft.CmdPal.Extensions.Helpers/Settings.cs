// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Windows.Foundation;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;

namespace Microsoft.CmdPal.Extensions.Helpers;

public sealed class Settings
{
    private readonly Dictionary<string, object> _settings = new();
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

    public event TypedEventHandler<object, Settings>? SettingsChanged;

    public void Add<T>(Setting<T> s)
    {
        _settings.Add(s.Key, s);
    }

    public T? GetSetting<T>(string key)
    {
        return _settings[key] is Setting<T> s ? s.Value : default;
    }

    public bool TryGetSetting<T>(string key, out T? val)
    {
        object? o;
        if (_settings.TryGetValue(key, out o))
        {
            if (o is Setting<T> s)
            {
                val = s.Value;
                return true;
            }
        }

        val = default;
        return false;
    }

    internal string ToFormJson()
    {
        var settings = _settings
            .Values
            .Where(s => s is ISettingsForm)
            .Select(s => s as ISettingsForm)
            .Where(s => s != null)
            .Select(s => s!);

        var bodies = string.Join(",", settings
            .Select(s => JsonSerializer.Serialize(s.ToDictionary(), _jsonSerializerOptions)));
        var datas = string.Join(",", settings.Select(s => s.ToDataIdentifier()));

        var json = $$"""
{
  "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
  "type": "AdaptiveCard",
  "version": "1.5",
  "body": [
      {{bodies}} 
  ],
  "actions": [
    {
      "type": "Action.Submit",
      "title": "Save",
      "data": {
        {{datas}}
      }
    }
  ]
}
""";
        return json;
    }

    public string ToJson()
    {
        var settings = _settings
            .Values
            .Where(s => s is ISettingsForm)
            .Select(s => s as ISettingsForm)
            .Where(s => s != null)
            .Select(s => s!);
        var content = string.Join(",\n", settings.Select(s => s.ToState()));
        return $"{{\n{content}\n}}";
    }

    public IForm[] ToForms()
    {
        return [new SettingsForm(this)];
    }

    public void Update(string data)
    {
        var formInput = JsonNode.Parse(data)?.AsObject();
        if (formInput == null)
        {
            return;
        }

        foreach (var key in _settings.Keys)
        {
            var value = _settings[key];
            if (value is ISettingsForm f)
            {
                f.Update(formInput);
            }
        }
    }

    internal void RaiseSettingsChanged()
    {
        var handlers = SettingsChanged;
        handlers?.Invoke(this, this);
    }

    /// <summary>
    /// Used to produce a path to a settings folder which your app can use.
    /// If your app is running packaged, this will return the redirected local
    /// app data path (Packages/<your_pfn>/LocalState). If not, it'll return
    /// %LOCALAPPDATA%\settingsFolderName.
    /// </summary>
    /// <param name="settingsFolderName"></param>
    /// <returns></returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1312:Variable names should begin with lower-case letter", Justification = "It's a Windows constants a ")]
    public static string BaseSettingsPath(string settingsFolderName)
    {
        var FOLDERID_LocalAppData = new Guid("F1B32785-6FBA-4FCF-9D55-7B8E7F157091");
        var hr = PInvoke.SHGetKnownFolderPath(
            FOLDERID_LocalAppData,
            (uint)KNOWN_FOLDER_FLAG.KF_FLAG_FORCE_APP_DATA_REDIRECTION,
            null,
            out var localAppDataFolder);

        if (hr.Succeeded)
        {
            var basePath = new string(localAppDataFolder.ToString());
            if (!IsPackaged())
            {
                basePath = Path.Combine(basePath, settingsFolderName);
            }

            return basePath;
        }
        else
        {
            throw Marshal.GetExceptionForHR(hr.Value)!;
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1312:Variable names should begin with lower-case letter", Justification = "It's a Windows constants a ")]
    public static bool IsPackaged()
    {
        uint buffersize = 0;
        var bytes = Array.Empty<byte>();

        // CsWinRT apparently won't generate this constant
        var APPMODEL_ERROR_NO_PACKAGE = (WIN32_ERROR)15700;
        unsafe
        {
            fixed (byte* p = bytes)
            {
                var win32Error = PInvoke.GetCurrentPackageId(ref buffersize, p);
                return win32Error != APPMODEL_ERROR_NO_PACKAGE;
            }
        }
    }
}
