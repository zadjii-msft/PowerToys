﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using Microsoft.Identity.Client;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.Windows.DevHome.SDK;
using Serilog;

namespace SchedulerExtension.DeveloperId;

public partial class DeveloperId : IDeveloperId
{
    public string LoginId { get; private set; }

    public string DisplayName { get; private set; }

    public string Email { get; private set; }

    public string Url { get; private set; }

    private readonly ILogger _log = Log.ForContext("SourceContext", nameof(DeveloperId));

    private Dictionary<Uri, VssConnection> Connections { get; } = new Dictionary<Uri, VssConnection>();

    public DeveloperId()
    {
        LoginId = string.Empty;
        DisplayName = string.Empty;
        Email = string.Empty;
        Url = string.Empty;
    }

    public DeveloperId(string loginId, string displayName, string email, string url)
    {
        LoginId = loginId;
        DisplayName = displayName;
        Email = email;
        Url = url;
    }

    ~DeveloperId()
    {
        LoginId = string.Empty;
        DisplayName = string.Empty;
        Email = string.Empty;
        Url = string.Empty;
        return;
    }

    // DeveloperIdInternal interfaces
    public VssCredentials? GetCredentials()
    {
        try
        {
            var authResult = DeveloperIdProvider.GetInstance().GetAuthenticationResultForDeveloperId(this) ?? throw new ArgumentException(this.LoginId);
            if (authResult != null)
            {
                return new VssAadCredential(new VssAadToken("Bearer", authResult.AccessToken));
            }
        }
        catch (MsalUiRequiredException ex)
        {
            _log.Error($"GetVssCredentials failed and requires user interaction {ex}");
            throw;
        }
        catch (MsalServiceException ex)
        {
            _log.Error($"GetVssCredentials failed with MSAL service error: {ex}");
            throw;
        }
        catch (MsalClientException ex)
        {
            _log.Error($"GetVssCredentials failed with MSAL client error: {ex}");
            throw;
        }
        catch (Exception ex)
        {
            _log.Error($"GetVssCredentials failed with error: {ex}");
            throw;
        }

        return null;
    }
}
