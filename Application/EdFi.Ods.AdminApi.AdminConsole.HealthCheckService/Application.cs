// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.Ods.AdminApi.AdminConsole.HealthCheckService.Features.AdminApi;
using EdFi.Ods.AdminApi.AdminConsole.HealthCheckService.Features.OdsApi;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json.Nodes;

namespace EdFi.Ods.AdminApi.AdminConsole.HealthCheckService;

public interface IApplication
{
    Task Run();
}

public class Application : IApplication, IHostedService
{
    private readonly ILogger _logger;
    private readonly IAdminApiCaller _adminApiCaller;
    private readonly IOdsApiCaller _odsApiCaller;

    public Application(ILogger logger, IAdminApiCaller adminApiCaller, IOdsApiCaller odsApiCaller)
    {
        _logger = logger;
        _adminApiCaller = adminApiCaller;
        _odsApiCaller = odsApiCaller;
    }

    public async Task Run()
    {
        /// Step 1. Get instances data from Admin API - Admin Console extension.
        var instances = await _adminApiCaller.GetInstancesAsync();

        if (instances == null || instances.Count() == 0)
        {
            _logger.LogInformation("No instances found on Admin Api.");
        }
        else
        {
            /// Step 2. For each instance, Get the HealthCheck data from ODS API
            foreach (var instance in instances)
            {
                _logger.LogInformation($"Processing instance with name: {instance.InstanceName}");
                var healthCheckData = await _odsApiCaller.ExecuteAsync(instance);

                _logger.LogInformation("HealCheck data obtained.");

                JsonObject healthCheckDocument = new();

                if (healthCheckData != null)
                {
                    healthCheckDocument.Add(new KeyValuePair<string, JsonNode?>("healthy", true));
                    foreach (var countPerEndpoint in healthCheckData)
                    {
                        healthCheckDocument.Add(new KeyValuePair<string, JsonNode?>(countPerEndpoint.OdsApiEndpointName, countPerEndpoint.OdsApiEndpointCount));
                    }
                }

                /// Step 3. Post the HealthCheck data to the Admin API
                var healCheckToPost = new AdminApiHealthCheckPost()
                {
                    TenantId = instance.TenantId,
                    InstanceId = instance.InstanceId,
                    EdOrgId = 1,
                    Document = healthCheckDocument.ToString(),
                };

                _logger.LogInformation("Posting HealthCheck data to Admin Api.");
                await _adminApiCaller.PostHealCheckAsync(healCheckToPost);
            }

            _logger.LogInformation("Process completed.");
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Run();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}
