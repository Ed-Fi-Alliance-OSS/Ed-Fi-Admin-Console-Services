// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.AdminConsole.HealthCheckService.Features.AdminApi;
using EdFi.AdminConsole.HealthCheckService.Helpers;
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

    public OdsApiCaller(ILogger logger, IOdsApiClient odsApiClient, IAppSettingsOdsApiEndpoints appSettingsOdsApiEndpoints)
    {
        _logger = logger;
        _odsApiClient = odsApiClient;
        _appSettingsOdsApiEndpoints = appSettingsOdsApiEndpoints;
    }

    public async Task<List<OdsApiEndpointNameCount>> GetHealthCheckDataAsync(AdminApiInstanceDocument instance)
    {
        var tasks = new List<Task<OdsApiEndpointNameCount>>();

        foreach (var appSettingsOdsApiEndpoint in _appSettingsOdsApiEndpoints)
        {
            var url = $"{instance.ResourcesUrl}{appSettingsOdsApiEndpoint}{Constants.OdsApiQueryParams}";

            tasks.Add(Task.Run(() => GetCountPerEndpointAsync(
                appSettingsOdsApiEndpoint, instance.AuthenticationUrl, instance.ClientId, instance.ClientSecret, url, appSettingsOdsApiEndpoint)));
        }

        return (await Task.WhenAll(tasks)).ToList();
    }

    protected async Task<OdsApiEndpointNameCount> GetCountPerEndpointAsync(string odsApiEndpoint, string authUrl, string clientId, string clientSecret, string odsEndpointUrl, string getInfo)
    {
        var result = new OdsApiEndpointNameCount()
        {
            OdsApiEndpointName = odsApiEndpoint,
        };
        var response = await _odsApiClient.Get(authUrl, clientId, clientSecret, odsEndpointUrl, getInfo);

        if (response != null && response.StatusCode == System.Net.HttpStatusCode.OK && response.Headers != null && response.Headers.Contains("total-count"))
            result.OdsApiEndpointCount = int.Parse(response.Headers.GetValues("total-count").First());

        return result;
    }
}
