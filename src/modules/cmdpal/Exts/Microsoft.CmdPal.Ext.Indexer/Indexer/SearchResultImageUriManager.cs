// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.IO;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace Microsoft.CmdPal.Ext.Indexer.Indexer;

internal sealed class SearchResultImageUriManager
{
    private readonly object _lockObject = new(); // Lock object for synchronization

    internal StorageItemThumbnail FolderThumbnail { get; private set; }

    internal Dictionary<string, StorageItemThumbnail> ExtensionThumbnailPathMapping { get; private set; }

    public SearchResultImageUriManager()
    {
        ExtensionThumbnailPathMapping = new Dictionary<string, StorageItemThumbnail>();
    }

    // TODO: check if has performance advantages
    public bool NeedProcessThumbnailForItem(string itemPath, bool isFolder)
    {
        lock (_lockObject)
        {
            if (isFolder && (FolderThumbnail == null))
            {
                return true;
            }
            else if (!isFolder)
            {
                var ext = Path.GetExtension(itemPath);
                if (ext != null && ext.Length > 0)
                {
                    ExtensionThumbnailPathMapping.TryGetValue(ext, out var thumbnail);
                    if (thumbnail == null)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public StorageItemThumbnail GetThumbnailForItem(string itemPath, bool isFolder)
    {
        lock (_lockObject)
        {
            if (isFolder)
            {
                return FolderThumbnail;
            }
            else
            {
                var ext = Path.GetExtension(itemPath);
                if (ext != null && ext.Length > 0)
                {
                    ExtensionThumbnailPathMapping.TryGetValue(ext, out var thumbnail);
                    if (thumbnail == null)
                    {
                        return thumbnail;
                    }
                }
            }
        }

        return null;
    }

    public bool InitThumbnail(string path, bool isFolder)
    {
        /*try
        {
            // Create the storage folder from the item
            StorageFolder folder = StorageFolder.GetFolderFromPathAsync(path).GetResults();

            // Get the thumbnail
            ThumbnailMode mode = ThumbnailMode.ListView;
            StorageItemThumbnail thumbnail = folder.GetThumbnailAsync(mode, 25).GetResults();

            lock (_lockObject)
            {
                if (isFolder)
                {
                    FolderThumbnail = thumbnail;
                }
                else
                {
                    var ext = Path.GetExtension(path);
                    if (ext != null && ext.Length > 0)
                    {
                        ExtensionThumbnailPathMapping.Add(ext, thumbnail);
                    }
                }
            }

            return true;
        }
        catch
        {
        }
*/
        return false;
    }
}
