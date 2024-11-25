// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using EdFi.AdminConsole.HealthCheckService.Features;
using EdFi.AdminConsole.HealthCheckService.Features.AdminApi;
using EdFi.AdminConsole.HealthCheckService.Features.OdsApi;
using EdFi.AdminConsole.HealthCheckService.Infrastructure;
using EdFi.Ods.AdminApi.HealthCheckService.UnitTests;
using EdFi.Ods.AdminApi.HealthCheckService.UnitTests.Helpers;
using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;

namespace EdFi.AdminConsole.HealthCheckService.UnitTests.Features.OdsApi;

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

        var adminApiInstance = Testing.AdminApiInstances.First();

        var encodedKeySecret = Encoding.ASCII.GetBytes($"{adminApiInstance.ClientId}:{adminApiInstance.ClientSecret}");

        var headers = new HttpResponseMessage().Headers;
        headers.Add("total-count", "5");

        A.CallTo(() => httpClient.SendAsync(
            adminApiInstance.AuthenticationUrl, HttpMethod.Post, A<FormUrlEncodedContent>.Ignored, new AuthenticationHeaderValue("Basic", Convert.ToBase64String(encodedKeySecret))))
            .Returns(new ApiResponse(HttpStatusCode.OK, "{ \"access_token\": \"123\"}"));

        A.CallTo(() => httpClient.SendAsync(adminApiInstance.ResourcesUrl, HttpMethod.Get, new AuthenticationHeaderValue("bearer", "123")))
            .Returns(new ApiResponse(HttpStatusCode.OK, string.Empty, headers));

        var odsApiClient = new OdsApiClient(httpClient, _logger, Testing.GetAppSettings(), new CommandArgs(_configuration));

        var response = await odsApiClient.OdsApiGet(
            adminApiInstance.AuthenticationUrl, adminApiInstance.ClientId, adminApiInstance.ClientSecret, adminApiInstance.ResourcesUrl, "Get Total Count from Ods Api");

        response.Headers.ShouldNotBeNull();
        response.Headers.Any(o => o.Key == "total-count").ShouldBe(true);
        response.Headers.GetValues("total-count").First().ShouldBe("5");
    }

    [Test]
    public async Task No_Access_Token()
    {
        var httpClient = A.Fake<IAppHttpClient>();

        var adminApiInstance = Testing.AdminApiInstances.First();

        var encodedKeySecret = Encoding.ASCII.GetBytes($"{adminApiInstance.ClientId}:{adminApiInstance.ClientSecret}");

        var headers = new HttpResponseMessage().Headers;
        headers.Add("total-count", "5");

        A.CallTo(() => httpClient.SendAsync(
            adminApiInstance.AuthenticationUrl, HttpMethod.Post, A<FormUrlEncodedContent>.Ignored, new AuthenticationHeaderValue("Basic", Convert.ToBase64String(encodedKeySecret))))
            .Returns(new ApiResponse(HttpStatusCode.InsufficientStorage, string.Empty));

        A.CallTo(() => httpClient.SendAsync(adminApiInstance.ResourcesUrl, HttpMethod.Get, new AuthenticationHeaderValue("bearer", "123")))
            .Returns(new ApiResponse(HttpStatusCode.OK, string.Empty, headers));

        var odsApiClient = new OdsApiClient(httpClient, _logger, Testing.GetAppSettings(), new CommandArgs(_configuration));

        var response = await odsApiClient.OdsApiGet(
            adminApiInstance.AuthenticationUrl, adminApiInstance.ClientId, adminApiInstance.ClientSecret, adminApiInstance.ResourcesUrl, "Get Total Count from Ods Api");

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
    }
}
