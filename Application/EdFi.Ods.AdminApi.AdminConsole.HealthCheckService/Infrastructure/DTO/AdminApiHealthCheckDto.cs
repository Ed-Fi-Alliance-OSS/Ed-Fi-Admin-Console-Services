// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

namespace EdFi.Ods.AdminApi.AdminConsole.HealthCheckService.Infrastructure.DTO;

public class AdminApiHealthCheckDto
{
    public string DocId { get; set; } = string.Empty;

    public string InstanceId { get; set; } = string.Empty;

    public string EdOrgId { get; set; } = string.Empty;
    
    public string TenantId { get; set; } = string.Empty;
    
    public string Document { get; set; } = string.Empty;
}
