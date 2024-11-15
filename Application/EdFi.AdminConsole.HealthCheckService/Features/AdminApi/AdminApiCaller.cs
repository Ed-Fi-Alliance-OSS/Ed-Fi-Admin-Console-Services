// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog.Core;
using System.Text;

namespace EdFi.AdminConsole.HealthCheckService.Features.AdminApi;

public interface IAdminApiCaller
{
    Task<IEnumerable<AdminApiInstance>> GetInstancesAsync();
    Task PostHealCheckAsync(AdminApiHealthCheckPost endpoints);
}

public class AdminApiCaller : IAdminApiCaller
{
    private readonly ILogger _logger;
    private IAdminApiClient _adminApiClient;
    private readonly AdminApiSettings _adminApiOptions;

    public AdminApiCaller(ILogger logger, IAdminApiClient adminApiClient, IOptions<AdminApiSettings> adminApiOptions)
    {
        _logger = logger;
        _adminApiClient = adminApiClient;
        _adminApiOptions = adminApiOptions.Value;
    }

    public async Task<IEnumerable<AdminApiInstance>> GetInstancesAsync()
    {
        if (IsAdminApiSettingsValid())
        {
            var instancesEndpoint = _adminApiOptions.ApiUrl + _adminApiOptions.AdminConsoleInstancesURI;
            var response = await _adminApiClient.Get(
                _adminApiOptions.AccessTokenUrl, _adminApiOptions.ClientId, _adminApiOptions.ClientSecret, instancesEndpoint, "Getting instances from Admin API - Admin Console extension");

            if (response.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
            {
                var instances = JsonConvert.DeserializeObject<IEnumerable<AdminApiInstance>>(response.Content);
                return instances ?? Enumerable.Empty<AdminApiInstance>();
            }
            return new List<AdminApiInstance>();
        }
        else {
            _logger.LogError("AdminApi Settings has not been set properly.");
            return new List<AdminApiInstance>(); 
        }
    }

    public async Task PostHealCheckAsync(AdminApiHealthCheckPost instanceHealthCheckData)
    {
        var healthCheckEndpoint = _adminApiOptions.ApiUrl + _adminApiOptions.AdminConsoleHealthCheckURI;

        var json = System.Text.Json.JsonSerializer.Serialize(instanceHealthCheckData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _adminApiClient.Post(
            _adminApiOptions.AccessTokenUrl, _adminApiOptions.ClientId, _adminApiOptions.ClientSecret, content, healthCheckEndpoint, "Posting HealthCheck to Admin API - Admin Console extension");

        if (response.StatusCode != System.Net.HttpStatusCode.Created)
        {
            _logger.LogError($"Not able to post HealthCheck data to Ods Api. Tenant Id: {instanceHealthCheckData.TenantId}.");
            _logger.LogError($"Status Code returned is: {response.StatusCode}.");
        }
    }

    private bool IsAdminApiSettingsValid()
    {
        if (string.IsNullOrEmpty(_adminApiOptions.AccessTokenUrl)
            || string.IsNullOrEmpty(_adminApiOptions.ApiUrl)
            || string.IsNullOrEmpty(_adminApiOptions.ClientId)
            || string.IsNullOrEmpty(_adminApiOptions.ClientSecret)
            || string.IsNullOrEmpty(_adminApiOptions.AdminConsoleInstancesURI)
            || string.IsNullOrEmpty(_adminApiOptions.AdminConsoleHealthCheckURI))
        { return false; }
        else { return true; }
    }
}
