// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.AdminConsole.HealthCheckService;
using EdFi.AdminConsole.HealthCheckService.Features.AdminApi;
using EdFi.AdminConsole.HealthCheckService.Features.OdsApi;
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
        adminApiSettings.AccessTokenUrl = "http://www.myserver.com/token";
        adminApiSettings.ApiUrl = "http://www.myserver.com";
        adminApiSettings.AdminConsoleInstancesURI = "/adminconsole/instances";
        adminApiSettings.AdminConsoleHealthCheckURI = "/adminconsole/healthcheck";
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

    public static List<OdsApiEndpointNameCount> HealthCheckData
    {
        get
        {
            return new List<OdsApiEndpointNameCount>
            {
                new OdsApiEndpointNameCount()
                {
                    OdsApiEndpointName = "firstEndPoint",
                    OdsApiEndpointCount = 3,
                    AnyErrros = false
                },
                new OdsApiEndpointNameCount()
                {
                    OdsApiEndpointName = "secondEndpoint",
                    OdsApiEndpointCount = 8,
                    AnyErrros = false
                },
                new OdsApiEndpointNameCount()
                {
                    OdsApiEndpointName = "thirdEndPoint",
                    OdsApiEndpointCount = 5,
                    AnyErrros = false
                }
            };
        }
    }

    public static List<AdminApiInstanceDocument> AdminApiInstances
    {
        get
        {
            return new List<AdminApiInstanceDocument>
            {
                new AdminApiInstanceDocument()
                {
                    InstanceId = 1,
                    TenantId = 1,
                    InstanceName = "instance 1",
                    ClientId = "one client",
                    ClientSecret = "one secret",
                    BaseUrl = "http://www.myserver.com",
                    AuthenticationUrl = "/connect/token",
                    ResourcesUrl = "/data/v3/ed-fi/",
                },
                new AdminApiInstanceDocument()
                {
                    InstanceId = 2,
                    TenantId = 2,
                    InstanceName = "instance 2",
                    ClientId = "another client",
                    ClientSecret = "another secret",
                    BaseUrl = "http://www.otherserver.com",
                    AuthenticationUrl = "/connect/token",
                    ResourcesUrl = "/data/v3/ed-fi/",
                }
            };
        }
    }

    public const string Instances =
    @"[{
        ""Document"": {
              ""instanceId"": 1,
              ""tenantId"": 1,
              ""instanceName"": ""instance 1"",
              ""clientId"": ""one client"",
              ""clientSecret"": ""one secret"",
              ""baseUrl"": ""http://www.myserver.com"",
              ""resourcesUrl"": ""/data/v3/ed-fi/"",
              ""authenticationUrl"": ""/connect/token""
        }},{
        ""Document"":{
            ""instanceId"": 2,
              ""tenantId"": 2,
              ""instanceName"": ""instance 2"",
              ""clientId"": ""another client"",
              ""clientSecret"": ""another secret"",
              ""baseUrl"": ""http://www.otherserver.com"",
              ""resourcesUrl"": ""/data/v3/ed-fi/"",
              ""authenticationUrl"": ""/connect/token""
        }
    }]";


    public static Dictionary<string, string> CommandArgsDicWithMultitenant = new Dictionary<string, string>
        {
            {"isMultiTenant", "true"},
            {"tenant", "Tenant1"},
            {"clientid", "SomeClientId"},
            {"clientsecret", "SomeClientSecret"}
        };

    public static Dictionary<string, string> CommandArgsDicWithSingletenant = new Dictionary<string, string>
        {
            {"isMultiTenant", "false"},
            {"clientid", "SomeClientId"},
            {"clientsecret", "SomeClientSecret"}
        };

    public static Dictionary<string, string> CommandArgsDicNoClientId = new Dictionary<string, string>
        {
            {"isMultiTenant", "false"},
            {"clientid", ""},
            {"clientsecret", "SomeClientSecret"}
        };

    public static Dictionary<string, string> CommandArgsDicNoClientSecret = new Dictionary<string, string>
        {
            {"isMultiTenant", "false"},
            {"clientid", "SomeClientId"},
            {"clientsecret", ""}
        };

    public static Dictionary<string, string> CommandArgsDicWithMultitenantNoTenant = new Dictionary<string, string>
        {
            {"isMultiTenant", "true"},
            {"tenant", ""},
            {"clientid", "SomeClientId"},
            {"clientsecret", "SomeClientSecret"}
        };
}
