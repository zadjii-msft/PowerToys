// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CmdPal.Ext.Apps.Programs;
using Microsoft.CmdPal.Ext.Apps.Storage;
using Microsoft.CmdPal.Ext.Apps.Utils;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace Microsoft.CmdPal.Ext.Apps;

public sealed partial class AllAppsPage : ListPage, IDisposable
{
    private IListItem[] allAppsSection = [];

    private AllAppsSettings settings = AllAppsSettings.Instance;

    private PackageRepository _packageRepository;

    private Win32ProgramFileSystemWatchers _win32ProgramRepositoryHelper;

    private Win32ProgramRepository _win32ProgramRepository;

    private bool _disposed;

    public AllAppsPage()
    {
        this.Name = "All Apps";
        this.Icon = new IconInfo("\ue71d");
        this.ShowDetails = true;
        this.IsLoading = true;
        this.PlaceholderText = "Search installed apps...";

        // This helper class initializes the file system watchers based on the locations to watch
        _win32ProgramRepositoryHelper = new Win32ProgramFileSystemWatchers();

        // Initialize the Win32ProgramRepository with the settings object
        _win32ProgramRepository = new Win32ProgramRepository(_win32ProgramRepositoryHelper.FileSystemWatchers.Cast<IFileSystemWatcherWrapper>().ToList(), settings, _win32ProgramRepositoryHelper.PathsToWatch);

        _packageRepository = new PackageRepository(new PackageCatalogWrapper());

        var a = Task.Run(() =>
        {
            _win32ProgramRepository.IndexPrograms();
        });

        var b = Task.Run(() =>
        {
            _packageRepository.IndexPrograms();
            UpdateUWPIconPath(ThemeHelper.GetCurrentTheme());
        });

        Task.WaitAll(a, b);

        this.IsLoading = false;

        settings.LastIndexTime = DateTime.Today;
    }

    private void UpdateUWPIconPath(Theme theme)
    {
        if (_packageRepository != null)
        {
            foreach (UWPApplication app in _packageRepository)
            {
                app.UpdateLogoPath(theme);
            }
        }
    }

    public override IListItem[] GetItems()
    {
        if (this.allAppsSection == null || allAppsSection.Length == 0)
        {
            var apps = GetPrograms();
            this.allAppsSection = apps
                            .Select((app) => new AppListItem(app))
                            .ToArray();
        }

        return allAppsSection;
    }

    internal List<AppItem> GetPrograms()
    {
        var uwpResults = _packageRepository
            .Where((application) => application.Enabled)
            .Select(app =>
                new AppItem()
                {
                    Name = app.Name,
                    Subtitle = app.Description,
                    Type = UWPApplication.Type(),
                    IcoPath = app.LogoType != LogoType.Error ? app.LogoPath : string.Empty,
                    DirPath = app.Location,
                    UserModelId = app.UserModelId,
                    IsPackaged = true,
                    Commands = app.Commands,
                });

        var win32Results = _win32ProgramRepository
            .Where((application) => application.Enabled && application.Valid)
            .Select(app =>
            {
                return new AppItem()
                {
                    Name = app.Name,
                    Subtitle = app.Description,
                    Type = app.Type(),
                    IcoPath = app.IcoPath,
                    ExePath = !string.IsNullOrEmpty(app.LnkFilePath) ? app.LnkFilePath : app.FullPath,
                    DirPath = app.Location,
                    Commands = app.GetCommands(),
                };
            });

        return uwpResults.Concat(win32Results).OrderBy(app => app.Name).ToList();
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _win32ProgramRepositoryHelper?.Dispose();
                _disposed = true;
            }
        }
    }
}
