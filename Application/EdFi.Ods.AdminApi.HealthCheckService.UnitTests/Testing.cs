﻿using EdFi.Ods.AdminApi.AdminConsole.HealthCheckService;
using EdFi.Ods.AdminApi.AdminConsole.HealthCheckService.Features.AdminApi;
using EdFi.Ods.AdminApi.AdminConsole.HealthCheckService.Features.OdsApi;
using Microsoft.Extensions.Options;

namespace EdFi.Ods.AdminApi.HealthCheckService.UnitTests;

public class Testing
{
    public static IOptions<AppSettings> GetAppSettings()
    {
        AppSettings appSettings = new AppSettings();
        IOptions<AppSettings> options = Options.Create(appSettings);
        return options;
    }

    public static IOptions<AdminApiSettings> GetAdminApiSettings()
    {
        AdminApiSettings adminApiSettings = new AdminApiSettings();
        IOptions<AdminApiSettings> options = Options.Create(adminApiSettings);
        return options;
    }

    public static IOptions<OdsApiSettings> GetOdsApiSettings()
    {
        OdsApiSettings odsApiSettings = new OdsApiSettings();
        odsApiSettings.Endpoints = Endpoints;
        IOptions<OdsApiSettings> options = Options.Create(odsApiSettings);

        return options;
    }

    public static List<string> Endpoints { get { return new List<string> { "firstEndPoint", "secondEndpoint", "thirdEndPoint" }; } }


    public static List<OdsApiEndpointNameCount> HealthCheckData { get
        {
            return new List<OdsApiEndpointNameCount>
            {
                new OdsApiEndpointNameCount()
                {
                    OdsApiEndpointName = "firstEndPoint",
                    OdsApiEndpointCount = 13
                },
                new OdsApiEndpointNameCount()
                {
                    OdsApiEndpointName = "secondEndpoint",
                    OdsApiEndpointCount = 14
                },
                new OdsApiEndpointNameCount()
                {
                    OdsApiEndpointName = "thirdEndPoint",
                    OdsApiEndpointCount = 13
                }
            };
        }
    }
    public static List<AdminApiInstance> AdminApiInstances
    {
        get
        {
            return new List<AdminApiInstance>
            {
                new AdminApiInstance()
                {
                    InstanceId = 1,
                    TenantId = 1,
                    InstanceName = "instance 1",
                    ClientId = "one client",
                    ClientSecret = "one secret",
                    BaseUrl = "one base url",
                    AuthenticationUrl = "one auth url",
                    ResourcesUrl = "one resourse url",
                },
                new AdminApiInstance()
                {
                    InstanceId = 2,
                    TenantId = 2,
                    InstanceName = "instance 2",
                    ClientId = "another client",
                    ClientSecret = "another secret",
                    BaseUrl = "another base url",
                    AuthenticationUrl = "another auth url",
                    ResourcesUrl = "another resourse url",
                }
            };
        }
    }

    public const string Instances =
    @"[{
          ""instanceId"": 1,
          ""tenantId"": 1,
          ""instanceName"": ""instance 1"",
          ""clientId"": ""one client"",
          ""clientSecret"": ""one secret"",
          ""baseUrl"": ""one base url"",
          ""resourcesUrl"": ""one resourse url"",
          ""authenticationUrl"": ""one auth url""
    },{
          ""instanceId"": 2,
          ""tenantId"": 2,
          ""instanceName"": ""instance 2"",
          ""clientId"": ""another client"",
          ""clientSecret"": ""another secret"",
          ""baseUrl"": ""another base url"",
          ""resourcesUrl"": ""another resourse url"",
          ""authenticationUrl"": ""another auth url""
    }]";
}
