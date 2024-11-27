// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Net;
using EdFi.AdminConsole.HealthCheckService.Features;
using EdFi.AdminConsole.HealthCheckService.Features.AdminApi;
using EdFi.AdminConsole.HealthCheckService.Infrastructure;
using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;

namespace EdFi.Ods.AdminApi.HealthCheckService.UnitTests.Features.AdminApi;

public class Given_an_admin_api
{
    [TestFixture]
    public class When_instances_are_returned_from_api : Given_an_admin_api
    {
        private ILogger<When_instances_are_returned_from_api> _logger;
        private IAdminApiCaller _adminApiCaller;
        private IAdminApiClient _adminApiClient;

        [SetUp]
        public void SetUp()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(Testing.CommandArgsDicWithSingletenant)
                .Build();

            _logger = A.Fake<ILogger<When_instances_are_returned_from_api>>();

            _adminApiClient = A.Fake<IAdminApiClient>();

            A.CallTo(() => _adminApiClient.AdminApiGet("Getting instances from Admin API - Admin Console extension"))
                .Returns(new ApiResponse(HttpStatusCode.OK, Testing.Instances));

            _adminApiCaller = new AdminApiCaller(_logger, _adminApiClient, Testing.GetAdminApiSettings(), new CommandArgs(configuration));
        }

        [Test]
        public async Task should_return_stronglytyped_instances()
        {
            var instances = await _adminApiCaller.GetInstancesAsync();
            instances.Count().ShouldBe(2);

            instances.First().InstanceId.ShouldBe(1);
            instances.First().TenantId.ShouldBe(1);
            instances.First().InstanceName.ShouldBe("instance 1");
            instances.First().ClientId.ShouldBe("one client");
            instances.First().ClientSecret.ShouldBe("one secret");
            instances.First().BaseUrl.ShouldBe("http://www.myserver.com");
            instances.First().ResourcesUrl.ShouldBe("/data/v3/ed-fi/");
            instances.First().AuthenticationUrl.ShouldBe("/connect/token");

            instances.ElementAt(1).InstanceId.ShouldBe(2);
            instances.ElementAt(1).TenantId.ShouldBe(2);
            instances.ElementAt(1).InstanceName.ShouldBe("instance 2");
            instances.ElementAt(1).ClientId.ShouldBe("another client");
            instances.ElementAt(1).ClientSecret.ShouldBe("another secret");
            instances.ElementAt(1).BaseUrl.ShouldBe("http://www.otherserver.com");
            instances.ElementAt(1).ResourcesUrl.ShouldBe("/data/v3/ed-fi/");
            instances.ElementAt(1).AuthenticationUrl.ShouldBe("/connect/token");
        }
    }
}
