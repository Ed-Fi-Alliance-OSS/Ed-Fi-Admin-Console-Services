// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.AdminConsole.HealthCheckService;
using EdFi.AdminConsole.HealthCheckService.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Reflection;

public class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = DefaultLogger.Build();

        try
        {
            await Run(args);
        }
        catch (Exception exception)
        {
            Log.Fatal(exception, "Host terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static async Task Run(string[] args)
    {
        Log.Information("Building host");
        var host = CreateHostBuilder(args);
        host.ConfigureServices(
            (context, services) => services.ConfigureTransformLoadServices(context.Configuration));

        host.UseConsoleLifetime();

        using var builtHost = host.Build();

        var cancellationTokenSource = new CancellationTokenSource();

        var assembly = Assembly.GetExecutingAssembly();
        var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                           ?.InformationalVersion;

        var logger = builtHost.Services.GetService<ILogger<Program>>();
        logger.LogInformation("{name} {version} Starting", assembly.GetName().Name, informationalVersion);

        Log.Information("Starting host");
        await builtHost.StartAsync(cancellationTokenSource.Token);
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host
            .CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(ConfigureAppConfig)
            .UseSerilog();

    private static void ConfigureAppConfig(HostBuilderContext context, IConfigurationBuilder config)
    {
        var runPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
        var loggingConfigFile = Path.Combine(runPath ?? "./", "logging.json");
        var env = context.HostingEnvironment;
        //config.Sources.Clear();

        config.AddJsonFile(loggingConfigFile, optional: false)
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

        config.AddEnvironmentVariables();
    }
}
