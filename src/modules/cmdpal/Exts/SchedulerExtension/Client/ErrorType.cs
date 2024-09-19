﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace SchedulerExtension.Client;

public enum ErrorType
{
    None,
    Unknown,
    InvalidArgument,
    EmptyUri,
    UriInvalidQuery,
    UriInvalidRepository,
    InvalidUri,
    NullDeveloperId,
    InvalidDeveloperId,
    QueryFailed,
    RepositoryFailed,
    FailedGettingClient,
    CredentialUIRequired,
    MsalServiceError,
    MsalClientError,
    GenericCredentialFailure,
    InitializeVssConnectionFailure,
    NullConnection,
    VssResourceNotFound,
}
