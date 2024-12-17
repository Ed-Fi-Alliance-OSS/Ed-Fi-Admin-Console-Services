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
using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;

namespace EdFi.AdminConsole.HealthCheckService.UnitTests.Features.AdminApi;

public class Given_an_admin_api
{
    private IConfiguration _configuration;
    private ILogger<Given_an_admin_api> _logger;

    [SetUp]
    public void SetUp()
    {
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(Testing.CommandArgsDicWithSingletenant)
            .Build();

        _logger = A.Fake<ILogger<Given_an_admin_api>>();
    }

    [TestFixture]
    public class When_instances_are_requested : Given_an_admin_api
    {
        [Test]
        public async Task should_return_successfully()
        {
            var httpClient = A.Fake<IAppHttpClient>();
            var instancesUrl = Testing.GetAdminApiSettings().Value.ApiUrl + Testing.GetAdminApiSettings().Value.AdminConsoleInstancesURI;

            A.CallTo(() => httpClient.SendAsync(Testing.GetAdminApiSettings().Value.AccessTokenUrl, HttpMethod.Post, A<FormUrlEncodedContent>.Ignored, null))
                .Returns(new ApiResponse(HttpStatusCode.OK, "{ \"access_token\": \"123\"}"));

            A.CallTo(() => httpClient.SendAsync(instancesUrl, HttpMethod.Get, null as StringContent, new AuthenticationHeaderValue("bearer", "123")))
                .Returns(new ApiResponse(HttpStatusCode.OK, Testing.Instances));

            var adminApiClient = new AdminApiClient(httpClient, _logger, Testing.GetAdminApiSettings(), new CommandArgs(_configuration));

            var InstancesReponse = await adminApiClient.AdminApiGet("Get Instances from Admin Api");

            InstancesReponse.Content.ShouldBeEquivalentTo(Testing.Instances);
        }
    }

    public class When_instances_are_requested_without_token : Given_an_admin_api
    {
        [Test]
        public async Task InternalServerError_is_returned()
        {
            var httpClient = A.Fake<IAppHttpClient>();

            A.CallTo(() => httpClient.SendAsync(Testing.GetAdminApiSettings().Value.AccessTokenUrl, HttpMethod.Post, A<FormUrlEncodedContent>.Ignored, null))
                .Returns(new ApiResponse(HttpStatusCode.InternalServerError, string.Empty));

            A.CallTo(() => httpClient.SendAsync(Testing.GetAdminApiSettings().Value.ApiUrl + Testing.GetAdminApiSettings().Value.AdminConsoleInstancesURI, HttpMethod.Get, null as StringContent, new AuthenticationHeaderValue("bearer", "123")))
                .Returns(new ApiResponse(HttpStatusCode.OK, Testing.Instances));

            var adminApiClient = new AdminApiClient(httpClient, _logger, Testing.GetAdminApiSettings(), new CommandArgs(_configuration));

            var getOnAdminApi = await adminApiClient.AdminApiGet("Get Instances from Admin Api");

            getOnAdminApi.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        }
    }
}
