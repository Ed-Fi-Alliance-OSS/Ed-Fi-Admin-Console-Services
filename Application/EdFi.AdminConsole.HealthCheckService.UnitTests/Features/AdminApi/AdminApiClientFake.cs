// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.AdminConsole.HealthCheckService.Features.AdminApi;
using EdFi.AdminConsole.HealthCheckService.Infrastructure;

namespace EdFi.Ods.AdminApi.HealthCheckService.UnitTests.Features.AdminApi;

public class AdminApiClientFake : IAdminApiClient
{
    public Task<ApiResponse> Get(string authenticationUrl, string clientId, string clientSecret, string odsEndpointUrl, string getInfo)
    {
        var response = new ApiResponse(System.Net.HttpStatusCode.OK, Testing.Instances);
        return Task.FromResult(response);
    }

    public Task<ApiResponse> Post(string authenticationUrl, string clientId, string clientSecret, StringContent content, string endpointUrl, string postInfo)
    {
        throw new NotImplementedException();
    }
}
