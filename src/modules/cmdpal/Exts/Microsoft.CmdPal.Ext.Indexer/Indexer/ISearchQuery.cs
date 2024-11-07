// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.CmdPal.Ext.Indexer.Indexer;

internal interface ISearchQuery
{
    public string QueryString { get; set; }

    public abstract void Init();

    public abstract void ExecuteSync();

    public abstract uint GetNumResults();
}
