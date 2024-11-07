// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.CmdPal.Ext.Indexer.Indexer;

public interface ISearchUXQuery
{
    public uint Cookie { get; set; }

    internal abstract void Init();

    internal abstract void Execute(string searchText, uint cookie);

    internal abstract SearchResult GetResult(uint idx);

    internal abstract void WaitForQueryCompletedEvent();

    internal abstract void CancelOutstandingQueries();
}
