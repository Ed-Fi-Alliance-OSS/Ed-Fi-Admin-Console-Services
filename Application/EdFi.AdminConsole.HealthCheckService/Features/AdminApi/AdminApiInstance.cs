// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

namespace EdFi.AdminConsole.HealthCheckService.Features.AdminApi;

public class AdminApiInstance
{
    public int TenantId { get; set; } = 0;
    public int InstanceId { get; set; } = 0;
    public int EdOrgId { get; set; } = 0;
    public string InstanceName { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string ResourcesUrl { get; set; } = string.Empty;
    public string AuthenticationUrl { get; set; } = string.Empty;
}
