﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Policy.WebApi;
using SchedulerExtension.DataManager;
using SchedulerExtension.DataModel.Enums;
using SchedulerExtension.Exceptions;
using Serilog;

namespace SchedulerExtension;

public partial class AzureDataManager
{
    // This is how frequently the DataStore update occurs.
    private static readonly TimeSpan _updateInterval = TimeSpan.FromMinutes(5);
    private static DateTime _lastUpdateTime = DateTime.MinValue;

    public static async Task Update()
    {
        // Only update per the update interval.
        // This is intended to be dynamic in the future.
        if (DateTime.UtcNow - _lastUpdateTime < _updateInterval)
        {
            return;
        }

        try
        {
            await UpdateDeveloperPullRequests();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Update failed unexpectedly.");
        }

        _lastUpdateTime = DateTime.UtcNow;
    }

    public static async Task UpdateDeveloperPullRequests()
    {
        var log = Log.ForContext("SourceContext", $"UpdateDeveloperPullRequests");
        log.Debug($"Executing UpdateDeveloperPullRequests");

        var cacheManager = CacheManager.GetInstance();
        if (cacheManager.UpdateInProgress)
        {
            log.Information("Cache is being updated, skipping Developer Pull Request Update");
            return;
        }

        var identifier = Guid.NewGuid();
        using var dataManager = CreateInstance(identifier.ToString()) ?? throw new DataStoreInaccessibleException();
        await dataManager.UpdatePullRequestsForLoggedInDeveloperIdsAsync(null, identifier);

        // Show any new notifications that were created from the pull request update.
        var notifications = dataManager.GetNotifications();
        foreach (var notification in notifications)
        {
            // Show notifications for failed checkruns for Developer users.
            if (notification.Type == NotificationType.PullRequestRejected || notification.Type == NotificationType.PullRequestApproved)
            {
                notification.ShowToast();
            }
        }
    }
}
