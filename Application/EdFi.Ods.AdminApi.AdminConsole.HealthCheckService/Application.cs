// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.Ods.AdminApi.AdminConsole.HealthCheckService.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace EdFi.Ods.AdminApi.AdminConsole.HealthCheckService;


public interface IApplication
{
    Task Run();
}

public class Application : IApplication, IHostedService
{
    private readonly IAdminApiCaller _adminApiCaller;

    public Application(IAdminApiCaller adminApiCaller, IOptions<AdminApiSettings> adminApiOptions)
    {
        _adminApiCaller = adminApiCaller;
    }
    public async Task Run()
    {
        var instances = await _adminApiCaller.ExecuteAsync();
        Console.WriteLine(instances);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Run();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}
