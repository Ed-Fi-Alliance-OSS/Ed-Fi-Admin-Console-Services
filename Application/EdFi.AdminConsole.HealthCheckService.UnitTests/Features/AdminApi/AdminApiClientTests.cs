// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Net;
using System.Net.Http.Headers;
using EdFi.AdminConsole.HealthCheckService.Features;
using EdFi.AdminConsole.HealthCheckService.Features.AdminApi;
using EdFi.AdminConsole.HealthCheckService.Infrastructure;
using EdFi.Ods.AdminApi.HealthCheckService.UnitTests;
using EdFi.Ods.AdminApi.HealthCheckService.UnitTests.Helpers;
using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;

namespace EdFi.AdminConsole.HealthCheckService.UnitTests.Features.AdminApi;

[TestFixture]
public class GivenASingleTenantEnvironment
{
    private ILogger<GivenASingleTenantEnvironment> _logger;
    private IConfiguration _configuration;

    [SetUp]
    public void SetUp()
    {
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(Testing.CommandArgsDic)
            .Build();

        _logger = A.Fake<ILogger<GivenASingleTenantEnvironment>>();
    }

    [Test]
    public async Task EverythingOk()
    {
        var httpClient = A.Fake<IAppHttpClient>();
        var instancesUrl = Testing.GetAdminApiSettings().Value.ApiUrl + Testing.GetAdminApiSettings().Value.AdminConsoleInstancesURI;

        A.CallTo(() => httpClient.SendAsync(Testing.GetAdminApiSettings().Value.AccessTokenUrl, HttpMethod.Post, A<FormUrlEncodedContent>.Ignored, null))
            .Returns(new ApiResponse(HttpStatusCode.OK, "{ \"access_token\": \"123\"}"));

        A.CallTo(() => httpClient.SendAsync(instancesUrl, HttpMethod.Get, new AuthenticationHeaderValue("bearer", "123")))
            .Returns(new ApiResponse(HttpStatusCode.OK, Testing.Instances));

        var adminApiClient = new AdminApiClient(httpClient, _logger, Testing.GetAdminApiSettings(), new CommandArgs(_configuration));

        var InstancesReponse = await adminApiClient.AdminApiGet("Get Instances from Admin Api");

        InstancesReponse.Content.ShouldBeEquivalentTo(Testing.Instances);
    }

    [Test]
    public async Task No_Access_Token()
    {
        var httpClient = A.Fake<IAppHttpClient>();

        A.CallTo(() => httpClient.SendAsync(Testing.GetAdminApiSettings().Value.AccessTokenUrl, HttpMethod.Post, A<FormUrlEncodedContent>.Ignored, null))
            .Returns(new ApiResponse(HttpStatusCode.InternalServerError, string.Empty));

        A.CallTo(() => httpClient.SendAsync(Testing.GetAdminApiSettings().Value.ApiUrl + Testing.GetAdminApiSettings().Value.AdminConsoleInstancesURI, HttpMethod.Get, new AuthenticationHeaderValue("bearer", "123")))
            .Returns(new ApiResponse(HttpStatusCode.OK, Testing.Instances));

        var adminApiClient = new AdminApiClient(httpClient, _logger, Testing.GetAdminApiSettings(), new CommandArgs(_configuration));

        var getOnAdminApi = await adminApiClient.AdminApiGet("Get Instances from Admin Api");

        getOnAdminApi.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
    }
}
