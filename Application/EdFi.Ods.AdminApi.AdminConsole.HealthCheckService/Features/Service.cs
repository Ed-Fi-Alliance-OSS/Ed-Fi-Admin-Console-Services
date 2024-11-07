// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

namespace EdFi.Ods.AdminApi.AdminConsole.HealthCheckService.Features;

public  class Service
{
    /// 1. Send requests to admin api to get ods instances.
    /// 2. Once the instances are collected, we have: ApiBaseUrl, clientId and clientSecret for each instance.
    /// 3. Once we have the list of instances we can start calling these ods instances to get health check data
}