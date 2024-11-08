// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.Ods.AdminApi.AdminConsole.HealthCheckService.Infrastructure.DTO;
using EdFi.Ods.AdminApi.AdminConsole.HealthCheckService.Infrastructure.Services.AdminApi;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace EdFi.Ods.AdminApi.AdminConsole.HealthCheckService.Features;

public interface IAdminApiCaller
{
    Task<IEnumerable<AdminApiInstanceDto>> ExecuteAsync();
}

public class AdminApiCaller : IAdminApiCaller   
{
    private IAdminApiClient _adminApi;
    private readonly AdminApiSettings _adminApiOptions;

    public AdminApiCaller(IAdminApiClient adminApi, IOptions<AdminApiSettings> adminApiOptions)
    {
        _adminApi = adminApi;
        _adminApiOptions = adminApiOptions.Value;
    }

    public async Task<IEnumerable<AdminApiInstanceDto>> ExecuteAsync()
    {
        return await GetInstances();
    }

    private async Task<IEnumerable<AdminApiInstanceDto>> GetInstances()
    {
        var instancesEndpoint = _adminApiOptions.ApiUrl + _adminApiOptions.AdminConsoleURI;
        var response = await _adminApi.Get(instancesEndpoint, "Getting instances from Admin API - Admin Console extension");

        if (response.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
        {
            return JsonConvert.DeserializeObject<IEnumerable<AdminApiInstanceDto>>(response.Content);
        }
        return new List<AdminApiInstanceDto>();
    }
}
