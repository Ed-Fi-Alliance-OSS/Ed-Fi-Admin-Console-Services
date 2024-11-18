// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.AdminConsole.HealthCheckService.Features.AdminApi;
using EdFi.AdminConsole.HealthCheckService.Features.OdsApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EdFi.AdminConsole.HealthCheckService;

public static class Startup
{
    public static IServiceCollection ConfigureTransformLoadServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
        services.Configure<AdminApiSettings>(configuration.GetSection("AdminApiSettings"));
        services.Configure<OdsApiSettings>(configuration.GetSection("OdsApiSettings"));

        services.AddSingleton<ILogger>(sp => sp.GetService<ILogger<Application>>());

        services.AddSingleton<IAppSettingsOdsApiEndpoints, AppSettingsOdsApiEndpoints>();

        services.AddTransient<IAdminApiClient, AdminApiClient>();
        services.AddTransient<IOdsApiClient, OdsApiClient>();

        services.AddTransient<IAdminApiCaller, AdminApiCaller>();
        services.AddTransient<IOdsApiCaller, OdsApiCaller>();

        services.AddTransient<IHostedService, Application>();

        return services;
    }
}
