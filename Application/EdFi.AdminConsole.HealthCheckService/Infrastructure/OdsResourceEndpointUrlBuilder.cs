// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.AdminConsole.HealthCheckService.Features;
using EdFi.AdminConsole.HealthCheckService.Helpers;

namespace EdFi.AdminConsole.HealthCheckService.Infrastructure;

public interface IOdsResourceEndpointUrlBuilder
{
    string GetOdsResourceEndpointUrl(string baseUrl, string relativeUri);

    string GetOdsAuthEndpointUrl(string baseUrl, string relativeUri);
}

public class OdsResourceEndpointUrlBuilder : IOdsResourceEndpointUrlBuilder
{
    private readonly ICommandArgs _commandArgs;

    public OdsResourceEndpointUrlBuilder(ICommandArgs commandArgs)
    {
        _commandArgs = commandArgs;
    }

    public string GetOdsResourceEndpointUrl(string baseUrl, string relativeUri)
    {
        if (_commandArgs.IsMultiTenant)
            return $"{baseUrl}/{_commandArgs.Tenant}{relativeUri}{Constants.OdsApiQueryParams}";
        else
            return $"{baseUrl}/{relativeUri}{Constants.OdsApiQueryParams}";
    }

    public string GetOdsAuthEndpointUrl(string baseUrl, string relativeUri)
    {
        if (_commandArgs.IsMultiTenant)
            return $"{baseUrl}/{_commandArgs.Tenant}{relativeUri}";
        else
            return $"{baseUrl}{relativeUri}";
    }
}
