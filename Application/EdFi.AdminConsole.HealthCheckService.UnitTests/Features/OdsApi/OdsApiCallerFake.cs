// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.AdminConsole.HealthCheckService.Features.AdminApi;
using EdFi.AdminConsole.HealthCheckService.Features.OdsApi;

namespace EdFi.Ods.AdminApi.HealthCheckService.UnitTests.Features.OdsApi;
public class OdsApiCallerFake : IOdsApiCaller
{
    public Task<List<OdsApiEndpointNameCount>> GetHealthCheckDataAsync(AdminApiInstanceDocument instance)
    {
        return Task.FromResult(Testing.HealthCheckData);
    }
}
