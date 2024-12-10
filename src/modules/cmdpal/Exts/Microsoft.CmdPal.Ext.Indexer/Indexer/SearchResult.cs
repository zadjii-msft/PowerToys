// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using Microsoft.CmdPal.Ext.Indexer.Indexer.Propsys;
using Microsoft.CmdPal.Ext.Indexer.Indexer.Utils;
using Microsoft.CmdPal.Ext.Indexer.Utils;

namespace Microsoft.CmdPal.Ext.Indexer.Indexer;

internal sealed class SearchResult
{
    public string ItemDisplayName { get; init; }

    public string ItemUrl { get; init; }

    public string LaunchUri { get; init; }

    public bool IsFolder { get; init; }

    public SearchResult()
    {
    }

    public SearchResult(string name, string url, string filePath, bool isFolder)
    {
        ItemDisplayName = name;
        ItemUrl = url;
        IsFolder = isFolder;

        if (LaunchUri == null || LaunchUri.Length == 0)
        {
            // Launch the file with the default app, so use the file path
            LaunchUri = filePath;
        }
    }

    public static SearchResult Create(IPropertyStore propStore)
    {
        var itemNameDisplay = default(PROPVARIANT);
        var itemUrl = default(PROPVARIANT);
        var kindText = default(PROPVARIANT);

        try
        {
            // Get item name display
            var key = Constants.PKEYItemNameDisplay;
            var hr = propStore.GetValue(ref key, out itemNameDisplay);
            if (hr != 0)
            {
                Logger.LogError("Get item name display error: " + hr);
                return null;
            }

            // Get item URL
            key = Constants.PKEYItemUrl;
            hr = propStore.GetValue(ref key, out itemUrl);
            if (hr != 0)
            {
                Logger.LogError("Get item URL error: " + hr);
                return null;
            }

            // Get kind text
            key = Constants.PKEYKindText;
            hr = propStore.GetValue(ref key, out kindText);
            if (hr != 0)
            {
                Logger.LogError("Get kind text error: " + hr);
                return null;
            }

            var isFolder = false;
            if (kindText.vt == (ushort)VarEnum.VT_LPWSTR && kindText.unionValue.pwszVal != IntPtr.Zero)
            {
                var kindString = Marshal.PtrToStringUni(kindText.unionValue.pwszVal);
                if (string.Equals(kindString, "Folder", StringComparison.OrdinalIgnoreCase))
                {
                    isFolder = true;
                }
            }

            var filePath = Marshal.PtrToStringUni(itemUrl.unionValue.pwszVal);
            if (filePath == null)
            {
                return null;
            }

            filePath = UrlToFilePathConverter.Convert(filePath);

            // Create the actual result object
            var searchResult = new SearchResult(
                Marshal.PtrToStringUni(itemNameDisplay.unionValue.pwszVal),
                Marshal.PtrToStringUni(itemUrl.unionValue.pwszVal),
                filePath,
                isFolder);

            return searchResult;
        }
        catch (Exception ex)
        {
            Logger.LogError("Failed to get property values from propStore.", ex);
            return null;
        }
        finally
        {
            itemNameDisplay.Dispose();
            itemUrl.Dispose();
            kindText.Dispose();
        }
    }
}
