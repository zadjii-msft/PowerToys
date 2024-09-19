// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Data;
using System.Linq;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace UnitConverterExtension;

public partial class UnitConverterListItem : ListItem, IFallbackHandler
{
    public UnitConverterListItem()
        : base(new NoOpCommand())
    {
        // In the case of the calculator, the ListItem itself is the fallback
        // handler, so that it can update it's Title and Subtitle accodingly.
        FallbackHandler = this;
        Title = "Unit Converter";
        Subtitle = "Provides unit conversion (e.g. 10 ft to m)";
    }

    public void UpdateQuery(string query)
    {
        if (string.IsNullOrEmpty(query) || query == "%%")
        {
            Title = "%%";
        }
        #pragma warning disable CA1310
        else if (query.StartsWith("%% "))
        #pragma warning restore CA1310
        {
            Title = ParseQuery(query.Remove(0, 3));
            Subtitle = query;
        }
        #pragma warning disable CA1310
        else if (query.StartsWith("%%"))
        #pragma warning restore CA1310
        {
            Title = ParseQuery(query.Remove(0, 2));
            Subtitle = query;
        }
        else
        {
            Title = string.Empty;
        }
    }

    private string ParseQuery(string query)
    {
        try
        {
            var result = InputInterpreter.Parse(query);
            if (result == null)
            {
                return string.Empty;
            }

            var x = UnitHandler.Convert(result);

            var resultString = x.FirstOrDefault<UnitConversionResult>().ToString(null) ?? string.Empty;

            return resultString;
        }
        catch (Exception e)
        {
            return $"Error: {e.Message}";
        }
    }
}
