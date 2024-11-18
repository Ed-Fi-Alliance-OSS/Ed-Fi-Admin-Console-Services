// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.AdminConsole.HealthCheckService.Features.AdminApi;

namespace EdFi.Ods.AdminApi.HealthCheckService.UnitTests.Features.AdminApi;
public class AdminApiCallerFake : IAdminApiCaller
{
    public Task<IEnumerable<AdminApiInstanceDocument>> GetInstancesAsync()
    {
        return Task.FromResult(Testing.AdminApiInstances.AsEnumerable());
    }

    public Task PostHealCheckAsync(AdminApiHealthCheckPost endpoints)
    {
        throw new NotImplementedException();
    }
}
