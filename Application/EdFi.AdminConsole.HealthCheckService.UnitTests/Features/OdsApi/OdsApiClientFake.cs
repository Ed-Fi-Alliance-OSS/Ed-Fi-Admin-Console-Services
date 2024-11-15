// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.AdminConsole.HealthCheckService.Features.OdsApi;
using EdFi.AdminConsole.HealthCheckService.Infrastructure;

namespace EdFi.Ods.AdminApi.HealthCheckService.UnitTests.Features.OdsApi;

public class OdsApiClientFake : IOdsApiClient
{
    public Task<ApiResponse> Get(string authenticationUrl, string clientId, string clientSecret, string resourcesUrl, string getInfo)
    {
        var httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        httpResponse.Headers.Add("total-count", getInfo.Length.ToString());
            
        var response = new ApiResponse(httpResponse.StatusCode, string.Empty, httpResponse.Headers);
        return Task.FromResult(response);
    }
}
