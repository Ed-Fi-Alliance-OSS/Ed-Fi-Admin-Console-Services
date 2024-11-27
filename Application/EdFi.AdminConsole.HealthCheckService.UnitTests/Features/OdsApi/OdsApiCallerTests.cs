// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Net;
using EdFi.AdminConsole.HealthCheckService.Features;
using EdFi.AdminConsole.HealthCheckService.Features.OdsApi;
using EdFi.AdminConsole.HealthCheckService.Helpers;
using EdFi.AdminConsole.HealthCheckService.Infrastructure;
using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;

namespace EdFi.Ods.AdminApi.HealthCheckService.UnitTests.Features.OdsApi;

public class Given_an_ods_api
{
    [TestFixture]
    public class When_HealthCheckData_is_returned_from_api : Given_an_ods_api
    {
        private ILogger<Given_an_ods_api> _logger;
        private IOdsApiClient _odsApiClient;
        private IOdsResourceEndpointUrlBuilder _odsResourceEndpointUrlBuilder;
        private IOdsApiCaller _odsApiCaller;

        [SetUp]
        public void SetUp()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(Testing.CommandArgsDicWithSingletenant)
                .Build();

            var commandArgs = new CommandArgs(configuration);

            _logger = A.Fake<ILogger<Given_an_ods_api>>();

            _odsApiClient = A.Fake<IOdsApiClient>();
            _odsResourceEndpointUrlBuilder = A.Fake<IOdsResourceEndpointUrlBuilder>();

            var adminApiInstance = Testing.AdminApiInstances.First();

            var httpResponse1 = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            httpResponse1.Headers.Add(Constants.TotalCountHeader, "3");

            var httpResponse2 = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            httpResponse2.Headers.Add(Constants.TotalCountHeader, "8");

            var httpResponse3 = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            httpResponse3.Headers.Add(Constants.TotalCountHeader, "5");

            A.CallTo(() => _odsApiClient.OdsApiGet(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, "http://www.myserver.com/data/v3/ed-fi/firstEndPoint?offset=0&limit=0&totalCount=true", "Get HealthCheck data from Ods Api"))
                .Returns(new ApiResponse(HttpStatusCode.OK, string.Empty, httpResponse1.Headers));

            A.CallTo(() => _odsApiClient.OdsApiGet(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, "http://www.myserver.com/data/v3/ed-fi/secondEndpoint?offset=0&limit=0&totalCount=true", "Get HealthCheck data from Ods Api"))
                .Returns(new ApiResponse(HttpStatusCode.OK, string.Empty, httpResponse2.Headers));

            A.CallTo(() => _odsApiClient.OdsApiGet(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, "http://www.myserver.com/data/v3/ed-fi/thirdEndPoint?offset=0&limit=0&totalCount=true", "Get HealthCheck data from Ods Api"))
                .Returns(new ApiResponse(HttpStatusCode.OK, string.Empty, httpResponse3.Headers));

            A.CallTo(() => _odsResourceEndpointUrlBuilder.GetOdsResourceEndpointUrl(adminApiInstance.BaseUrl, $"{adminApiInstance.ResourcesUrl}firstEndPoint"))
                .Returns($"{adminApiInstance.BaseUrl}{adminApiInstance.ResourcesUrl}firstEndPoint{Constants.OdsApiQueryParams}");

            A.CallTo(() => _odsResourceEndpointUrlBuilder.GetOdsResourceEndpointUrl(adminApiInstance.BaseUrl, $"{adminApiInstance.ResourcesUrl}secondEndpoint"))
                .Returns($"{adminApiInstance.BaseUrl}{adminApiInstance.ResourcesUrl}secondEndpoint{Constants.OdsApiQueryParams}");

            A.CallTo(() => _odsResourceEndpointUrlBuilder.GetOdsResourceEndpointUrl(adminApiInstance.BaseUrl, $"{adminApiInstance.ResourcesUrl}thirdEndPoint"))
                .Returns($"{adminApiInstance.BaseUrl}{adminApiInstance.ResourcesUrl}thirdEndPoint{Constants.OdsApiQueryParams}");

            _odsApiCaller = new OdsApiCaller(_logger, _odsApiClient, new AppSettingsOdsApiEndpoints(Testing.GetOdsApiSettings()), commandArgs, _odsResourceEndpointUrlBuilder);
        }

        [Test]
        public async Task should_return_stronglytyped_healthCheck_data()
        {
            var healthCheckData = await _odsApiCaller.GetHealthCheckDataAsync(Testing.AdminApiInstances.First());
            healthCheckData.ShouldBeEquivalentTo(Testing.HealthCheckData);
        }
    }
}
