// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.Ods.AdminApi.AdminConsole.HealthCheckService.Features.AdminApi;
using EdFi.Ods.AdminApi.AdminConsole.HealthCheckService.Infrastructure;

namespace EdFi.Ods.AdminApi.HealthCheckService.UnitTests.Features.AdminApi;

public class AdminApiClientFake : IAdminApiClient
{
    public Task<ApiResponse> Get(string endpointUrl, string getInfo)
    {
        var response = new ApiResponse(System.Net.HttpStatusCode.OK, Testing.Instances);
        return Task.FromResult(response);
    }

    public Task<ApiResponse> Post(StringContent content, string endpointUrl, string postInfo)
    {
        throw new NotImplementedException();
    }
}
