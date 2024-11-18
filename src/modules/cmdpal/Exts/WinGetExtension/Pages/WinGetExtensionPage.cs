// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Microsoft.Management.Deployment;
using WindowsPackageManager.Interop;

namespace WinGetExtension;

internal sealed partial class WinGetExtensionPage : ListPage
{
    private readonly WindowsPackageManagerStandardFactory _winGetFactory;

    public WinGetExtensionPage()
    {
        Icon = new(string.Empty);
        Name = "WinGet for the Command Palette";

        _winGetFactory = new WindowsPackageManagerStandardFactory();
    }

    public override IListItem[] GetItems()
    {
        var packagesAsync = DoSearchAsync();
        packagesAsync.ConfigureAwait(false);
        var packages = packagesAsync.Result;
        return packages.Count == 0
            ? [
                new ListItem(new NoOpCommand()) { Title = "No packages found" }
            ]
            : packages.Select(p =>
            new ListItem(new NoOpCommand())
            {
                Title = p.Name,
                Subtitle = p.Id,
                Tags = [new Tag() { Text = p.AvailableVersions[0].Version }],
            }).ToArray();
    }

    private async Task<List<CatalogPackage>> DoSearchAsync()
    {
        var query = "wt";
        var results = new List<CatalogPackage>();

        // Create Package Manager and get available catalogs
        var manager = _winGetFactory.CreatePackageManager();
        var availableCatalogs = manager.GetPackageCatalogs();

        var nameFilter = _winGetFactory.CreatePackageMatchFilter();
        nameFilter.Field = Microsoft.Management.Deployment.PackageMatchField.Name;
        nameFilter.Value = query;

        // filterList.Filters.Add(nameFilter);
        var idFilter = _winGetFactory.CreatePackageMatchFilter();
        idFilter.Field = Microsoft.Management.Deployment.PackageMatchField.Id;
        idFilter.Value = query;
        idFilter.Option = PackageFieldMatchOption.ContainsCaseInsensitive;

        // filterList.Filters.Add(idFilter);
        var monikerFilter = _winGetFactory.CreatePackageMatchFilter();
        monikerFilter.Field = Microsoft.Management.Deployment.PackageMatchField.Moniker;
        monikerFilter.Value = query;
        monikerFilter.Option = PackageFieldMatchOption.ContainsCaseInsensitive;

        var commandFilter = _winGetFactory.CreatePackageMatchFilter();
        commandFilter.Field = Microsoft.Management.Deployment.PackageMatchField.Command;
        commandFilter.Value = query;
        commandFilter.Option = PackageFieldMatchOption.ContainsCaseInsensitive;

        // filterList.Filters.Add(monikerFilter);
        PackageMatchFilter[] filters = [
            nameFilter,
            idFilter,
            commandFilter,
            monikerFilter
          ];

        foreach (var catalog in availableCatalogs.ToArray())
        {
            foreach (var filter in filters)
            {
                // Create a filter to search for packages
                var filterList = _winGetFactory.CreateFindPackagesOptions();

                // Add the query to the filter
                filterList.Filters.Add(filter);

                // Find the packages with the filters
                var searchResults = await catalog.Connect().PackageCatalog.FindPackagesAsync(filterList);
                foreach (var match in searchResults.Matches.ToArray())
                {
                    // Print the packages
                    var package = match.CatalogPackage;

                    // Console.WriteLine(Package.Name);
                    results.Add(package);
                }
            }
        }

        return results;
    }
}
