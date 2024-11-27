// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Text;
using EdFi.AdminConsole.HealthCheckService.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace EdFi.AdminConsole.HealthCheckService.Features.AdminApi;

public interface IAdminApiCaller
{
    Task<IEnumerable<AdminApiInstanceDocument>> GetInstancesAsync();
    Task PostHealCheckAsync(AdminApiHealthCheckPost endpoints);
}

public class AdminApiCaller : IAdminApiCaller
{
    private readonly ILogger _logger;
    private IAdminApiClient _adminApiClient;
    private readonly IAdminApiSettings _adminApiOptions;
    private readonly ICommandArgs _commandArgs;

    public AdminApiCaller(ILogger logger, IAdminApiClient adminApiClient, IOptions<AdminApiSettings> adminApiOptions, ICommandArgs commandArgs)
    {
        _logger = logger;
        _adminApiClient = adminApiClient;
        _adminApiOptions = adminApiOptions.Value;
        _commandArgs = commandArgs;
    }

    public async Task<IEnumerable<AdminApiInstanceDocument>> GetInstancesAsync()
    {
        if (AdminApiConnectioDataValidator.IsValid(_logger, _adminApiOptions, _commandArgs))
        {
            var instancesEndpoint = _adminApiOptions.ApiUrl + _adminApiOptions.AdminConsoleInstancesURI;
            var response = await _adminApiClient.AdminApiGet("Getting instances from Admin API - Admin Console extension");

            if (response.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
            {
                var instances = JsonConvert.DeserializeObject<IEnumerable<AdminApiInstance>>(response.Content);
                if (instances != null)
                {
                    return instances.Select(i => i.Document) ?? Enumerable.Empty<AdminApiInstanceDocument>();
                }
            }
            return new List<AdminApiInstanceDocument>();
        }
        else
        {
            _logger.LogError("AdminApi Settings has not been set properly.");
            return new List<AdminApiInstanceDocument>();
        }
    }

    public async Task PostHealCheckAsync(AdminApiHealthCheckPost instanceHealthCheckData)
    {
        var healthCheckEndpoint = _adminApiOptions.ApiUrl + _adminApiOptions.AdminConsoleHealthCheckURI;

        var instanceHealthCheckDataJson = System.Text.Json.JsonSerializer.Serialize(instanceHealthCheckData);
        var instanceHealthCheckDataContent = new StringContent(instanceHealthCheckDataJson, Encoding.UTF8, "application/json");

        var response = await _adminApiClient.AdminApiPost(instanceHealthCheckDataContent, "Posting HealthCheck to Admin API - Admin Console extension");

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
            || string.IsNullOrEmpty(_adminApiOptions.AdminConsoleInstancesURI)
            || string.IsNullOrEmpty(_adminApiOptions.AdminConsoleHealthCheckURI)
            || string.IsNullOrEmpty(_commandArgs.ClientId)
            || string.IsNullOrEmpty(_commandArgs.ClientSecret)
            )
        { return false; }
        else
        { return true; }
    }
}
