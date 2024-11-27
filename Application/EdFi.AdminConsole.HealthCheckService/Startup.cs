// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Net.Http;
using EdFi.AdminConsole.HealthCheckService.Features;
using EdFi.AdminConsole.HealthCheckService.Features.AdminApi;
using EdFi.AdminConsole.HealthCheckService.Features.OdsApi;
using EdFi.AdminConsole.HealthCheckService.Infrastructure;
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
        services.AddSingleton<ICommandArgs, CommandArgs>();

        services.AddTransient<IHttpRequestMessageBuilder, HttpRequestMessageBuilder>();
        services.AddTransient<IOdsResourceEndpointUrlBuilder, OdsResourceEndpointUrlBuilder>();

        services.AddTransient<IAdminApiClient, AdminApiClient>();
        services.AddTransient<IOdsApiClient, OdsApiClient>();

        services.AddTransient<IAdminApiCaller, AdminApiCaller>();
        services.AddTransient<IOdsApiCaller, OdsApiCaller>();

        services.AddTransient<IHostedService, Application>();

        //services.AddTransient<IAdminApiClientNew, AdminApiClient>();

        //services.AddHttpClient<IAppHttpClient, AppHttpClient>();

        services.AddHttpClient<IAppHttpClient, AppHttpClient>("AppHttpClient", x =>
        {
            x.Timeout = TimeSpan.FromSeconds(5);
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler();
            if (configuration.GetSection("AppSettings")["IgnoresCertificateErrors"].ToLower() == "true")
            {
                return IgnoresCertificateErrorsHandler();
            }
            return handler;
        });

        services.AddTransient<AdminApiClient>();

        return services;
    }

    private static HttpClientHandler IgnoresCertificateErrorsHandler()
    {
        var handler = new HttpClientHandler();
        handler.ClientCertificateOptions = ClientCertificateOption.Manual;
        handler.ServerCertificateCustomValidationCallback =
            (httpRequestMessage, cert, cetChain, policyErrors) =>
            {
                return true;
            };

        return handler;
    }
}
