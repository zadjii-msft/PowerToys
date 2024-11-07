// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Windows.Storage.FileProperties;

namespace Microsoft.CmdPal.Ext.Indexer.Indexer;

internal sealed class SearchResult
{
    public string ItemDisplayName { get; init; }

    public string ItemUrl { get; init; }

    public string LaunchUri { get; init; }

    public bool IsFolder { get; init; }

    public bool CanDisplay { get; private set; }

    public StorageItemThumbnail Thumbnail { get; private set; }

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
            TryGetThumbnailForStorageItem();
        }
    }

    private void TryGetThumbnailForStorageItem()
    {
        if (Program.ImageUriManager.NeedProcessThumbnailForItem(LaunchUri, IsFolder))
        {
            Program.ImageUriManager.InitThumbnail(LaunchUri, IsFolder);
        }

        Thumbnail = Program.ImageUriManager.GetThumbnailForItem(LaunchUri, IsFolder);
        CanDisplay = Thumbnail != null;
    }
}
