// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.Ods.AdminApi.AdminConsole.HealthCheckService.Helpers;
using EdFi.Ods.AdminApi.AdminConsole.HealthCheckService.Infrastructure.DTO;
using EdFi.Ods.AdminApi.AdminConsole.HealthCheckService.Infrastructure.Services.OdsApi;
using System.Text.Json.Nodes;

namespace EdFi.Ods.AdminApi.AdminConsole.HealthCheckService.Features;

public interface IOdsApiCaller
{
    Task ExecuteAsync(IEnumerable<AdminApiInstanceDto> instances);
}

public class OdsApiCaller : IOdsApiCaller
{
    private IOdsApiClient _odsApiClient;
    private IOdsApiEndpoints _odsApiEndpoints;

    public OdsApiCaller(IOdsApiClient odsApiClient, IOdsApiEndpoints odsApiEndpoints)
    {
        _odsApiClient = odsApiClient;
        _odsApiEndpoints = odsApiEndpoints;
    }

    public async Task ExecuteAsync(IEnumerable<AdminApiInstanceDto> instances)
    {
        KeyValuePair<string, int>[]? countsPerEndpoint = null;

        foreach (var instance in instances)
        {
            var odsApiEndpoints = new List<string>();

            foreach (var endpoint in _odsApiEndpoints)
            {
                odsApiEndpoints.Add($"{instance.ResourcesUrl}{endpoint}{Constants.OdsApiQueryParams}");
            }

            var tasks = new List<Task<KeyValuePair<string, int>>>();

            foreach (var url in odsApiEndpoints)
            {
                tasks.Add(Task.Run(() => GetCountPerEndpointAsync(url, instance.AuthenticationUrl, instance.ClientSecret, instance.ClientSecret, instance.ResourcesUrl, "get info")));
            }

            countsPerEndpoint = await Task.WhenAll(tasks);

            foreach (var countPerEndpoint in countsPerEndpoint)
            {
                Console.WriteLine(countPerEndpoint.Key);
                Console.WriteLine(countPerEndpoint.Value);
            }

            Console.WriteLine("All requests completed.");
        }

        if (countsPerEndpoint != null)
        {
            foreach (var countPerEndpoint in countsPerEndpoint)
            {
                var endpointWithCountJsonObjectString = BuildHealthCheckDocument(countPerEndpoint);
            }
        }
    }

    private async Task<KeyValuePair<string, int>> GetCountPerEndpointAsync(string url, string authenticationUrl, string clientId, string clientSecret, string resourcesUrl, string getInfo)
    {
        var response = await _odsApiClient.Get(authenticationUrl, clientId, clientSecret, resourcesUrl, getInfo);

        if (response != null && response.StatusCode == System.Net.HttpStatusCode.OK && response.Headers != null && response.Headers.Contains("total-count"))
        {
            return new KeyValuePair<string, int>(url,int.Parse(response.Headers.GetValues("total-count").First()));
        }
        return new KeyValuePair<string, int>("url", 0);
    }

    private string BuildHealthCheckDocument(KeyValuePair<string, int> endpointWithCount)
    {
        JsonObject endpointWithCountJsonObject = new();
        endpointWithCountJsonObject.Add(new KeyValuePair<string, JsonNode?>(endpointWithCount.Key, endpointWithCount.Value));
        return endpointWithCountJsonObject.ToString();
    }
}
