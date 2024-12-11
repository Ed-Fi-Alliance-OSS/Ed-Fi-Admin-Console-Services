// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.AdminConsole.HealthCheckService.Features.AdminApi;
using EdFi.AdminConsole.HealthCheckService.Helpers;
using EdFi.AdminConsole.HealthCheckService.Infrastructure;
using Microsoft.Extensions.Logging;

namespace EdFi.AdminConsole.HealthCheckService.Features.OdsApi;

public interface IOdsApiCaller
{
    Task<List<OdsApiEndpointNameCount>> GetHealthCheckDataAsync(AdminApiInstanceDocument instance);
}

public class OdsApiCaller : IOdsApiCaller
{
    private readonly ILogger _logger;
    private IOdsApiClient _odsApiClient;
    private IAppSettingsOdsApiEndpoints _appSettingsOdsApiEndpoints;
    private readonly ICommandArgs _commandArgs;
    private IOdsResourceEndpointUrlBuilder _odsResourceEndpointUrlBuilder;

    public OdsApiCaller(ILogger logger, IOdsApiClient odsApiClient, IAppSettingsOdsApiEndpoints appSettingsOdsApiEndpoints, ICommandArgs commandArgs, IOdsResourceEndpointUrlBuilder odsResourceEndpointUrlBuilder)
    {
        _logger = logger;
        _odsApiClient = odsApiClient;
        _appSettingsOdsApiEndpoints = appSettingsOdsApiEndpoints;
        _commandArgs = commandArgs;
        _odsResourceEndpointUrlBuilder = odsResourceEndpointUrlBuilder;
    }

    public async Task<List<OdsApiEndpointNameCount>> GetHealthCheckDataAsync(AdminApiInstanceDocument instance)
    {
        var tasks = new List<Task<OdsApiEndpointNameCount>>();

        foreach (var appSettingsOdsApiEndpoint in _appSettingsOdsApiEndpoints)
        {
            var odsResourceEndpointUrl = _odsResourceEndpointUrlBuilder.GetOdsResourceEndpointUrl(instance.BaseUrl, $"{instance.ResourcesUrl}{appSettingsOdsApiEndpoint}");

            var odsAuthEndpointUrl = _odsResourceEndpointUrlBuilder.GetOdsAuthEndpointUrl(instance.BaseUrl, instance.AuthenticationUrl);

            tasks.Add(Task.Run(() => GetCountPerEndpointAsync(
                appSettingsOdsApiEndpoint, odsAuthEndpointUrl, instance.ClientId, instance.ClientSecret, odsResourceEndpointUrl)));
        }

        return (await Task.WhenAll(tasks)).ToList();
    }

    protected async Task<OdsApiEndpointNameCount> GetCountPerEndpointAsync(string odsApiEndpoint, string authUrl, string clientId, string clientSecret, string odsEndpointUrl)
    {
        var result = new OdsApiEndpointNameCount()
        {
            OdsApiEndpointName = odsApiEndpoint,
        };
        var response = await _odsApiClient.OdsApiGet(authUrl, clientId, clientSecret, odsEndpointUrl, "Get HealthCheck data from Ods Api");

        if (response != null && response.StatusCode == System.Net.HttpStatusCode.OK && response.Headers != null && response.Headers.Contains(Constants.TotalCountHeader))
            result.OdsApiEndpointCount = int.Parse(response.Headers.GetValues(Constants.TotalCountHeader).First());
        else
            result.AnyErrros = true;

        return result;
    }
}
