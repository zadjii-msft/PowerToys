// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Windows.Storage;
using Windows.System;

namespace Microsoft.CmdPal.Ext.Indexer.Indexer.Utils;

internal sealed class QueryStringBuilder
{
    private const string Select = "SELECT";
    private const string FromIndex = "FROM SystemIndex WHERE";
    private const string ScopeFileConditions = " SCOPE='file:' AND SCOPE <> 'file://C:/users/tltay'";
    private const string ScopeEmailConditions = " OR SCOPE='mapi:' OR SCOPE='mapi16:'";
    private const string OrderConditions = " ORDER BY System.Search.Rank, System.DateModified, System.ItemNameDisplay DESC";

    private string usersScope;

    public string Scope { get; set; } // can be overriden to provide a custom scope

    public List<string> Properties { get; set; } = new()
    {
        "System.ItemUrl",
        "System.ItemNameDisplay",
        "path",
        "System.Search.EntryID",
        "System.Kind",
        "System.KindText",
        "System.Search.GatherTime",
        "System.Search.QueryPropertyHits",
    };

    public string GenerateSingleUserScope()
    {
        // Get all the users, and filter out the one that is not us
        StringBuilder scopeStr = new StringBuilder();
        var users = User.FindAllAsync().GetAwaiter().GetResult();
        foreach (var user in users)
        {
            var foundUserProfileDir = UserDataPaths.GetForUser(user).Profile;
            scopeStr.Append(CultureInfo.CurrentCulture, $" AND SCOPE <> {foundUserProfileDir}");
        }

        return scopeStr.ToString();
    }

    public string GenerateProperties()
    {
        return string.Join(", ", Properties) + " ";
    }

    public string GenerateSelectQueryWithScope()
    {
        if (string.IsNullOrEmpty(usersScope))
        {
            usersScope = GenerateSingleUserScope();
        }

        StringBuilder queryStr = new StringBuilder(Select);
        queryStr.Append(' ');
        queryStr.Append(GenerateProperties());
        queryStr.Append(FromIndex);
        queryStr.Append(" (");

        if (string.IsNullOrEmpty(Scope))
        {
            queryStr.Append(ScopeFileConditions);
        }
        else
        {
            queryStr.Append(Scope);
        }

        queryStr.Append(')');
        return queryStr.ToString();
    }

    public string GeneratePrimingQuery()
    {
        var queryStr = GenerateSelectQueryWithScope();
        queryStr += OrderConditions;
        return queryStr;
    }

    public List<string> GenerateSearchQueryTokens(string searchText)
    {
        return searchText.Split(' ').ToList();
    }

    public string GenerateQuery(string searchText, uint whereId, bool contentSearchEnabled = false)
    {
        StringBuilder queryStr = new StringBuilder(GenerateSelectQueryWithScope());
        var lenSearchText = searchText.Length;

        // Filter by item name display only
        if (lenSearchText > 0)
        {
            queryStr.Append(" AND (CONTAINS(System.ItemNameDisplay, '\"");
            queryStr.Append(searchText);
            queryStr.Append("*\"')");
        }

        List<string> tokens = GenerateSearchQueryTokens(searchText);

        // Are we searching contents?
        if (contentSearchEnabled && lenSearchText > 0)
        {
            queryStr.Append(" OR (");
            for (var i = 0; i < tokens.Count; ++i)
            {
                queryStr.Append("CONTAINS(*, '\"");
                queryStr.Append(tokens[i]);
                queryStr.Append("*\"')");

                if (i < (tokens.Count - 1))
                {
                    queryStr.Append(" AND ");
                }
            }

            queryStr.Append(')');
        }

        // Group the contains
        if (lenSearchText > 0)
        {
            queryStr.Append(')');
        }

        // Always add reuse where to the query
        queryStr.Append(" AND ReuseWhere(");
        queryStr.Append(whereId.ToString(CultureInfo.CurrentCulture));
        queryStr.Append(')');

        queryStr.Append(OrderConditions);

        return queryStr.ToString();
    }
}
