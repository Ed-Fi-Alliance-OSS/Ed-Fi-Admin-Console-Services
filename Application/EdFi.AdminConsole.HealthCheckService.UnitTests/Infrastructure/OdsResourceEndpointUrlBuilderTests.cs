// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.AdminConsole.HealthCheckService.Features;
using EdFi.AdminConsole.HealthCheckService.Infrastructure;
using EdFi.Ods.AdminApi.HealthCheckService.UnitTests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;

namespace EdFi.AdminConsole.HealthCheckService.UnitTests.Infrastructure;

public class Given_One_Set_Of_Command_Args
{
    [TestFixture]
    public class When_it_has_been_set_with_single_tenant : Given_One_Set_Of_Command_Args
    {
        private ILogger<When_it_has_been_set_with_single_tenant> _logger;
        private IConfiguration _configuration;

        [SetUp]
        public void SetUp()
        {
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(Testing.CommandArgsDicWithSingletenant)
                .Build();
        }

        [Test]
        public void should_return_resource_url_without_tenant()
        {
            var odsResourceEndpointUrlBuilder = new OdsResourceEndpointUrlBuilder(new CommandArgs(_configuration));
            var url = odsResourceEndpointUrlBuilder.GetOdsResourceEndpointUrl("http://www.otherserver.com", "/oneendpoint");

            url.ShouldBe("http://www.otherserver.com//oneendpoint?offset=0&limit=0&totalCount=true");
        }

        [Test]
        public void should_return_auth_url_without_tenant()
        {
            var odsResourceEndpointUrlBuilder = new OdsResourceEndpointUrlBuilder(new CommandArgs(_configuration));
            var url = odsResourceEndpointUrlBuilder.GetOdsAuthEndpointUrl("http://www.otherserver.com", "/oneendpoint");

            url.ShouldBe("http://www.otherserver.com/oneendpoint");
        }
    }

    [TestFixture]
    public class When_it_has_been_set_with_multi_tenant : Given_One_Set_Of_Command_Args
    {
        private ILogger<When_it_has_been_set_with_multi_tenant> _logger;
        private IConfiguration _configuration;

        [SetUp]
        public void SetUp()
        {
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(Testing.CommandArgsDicWithMultitenant)
                .Build();
        }

        [Test]
        public void should_return_resource_url_with_tenant()
        {
            var odsResourceEndpointUrlBuilder = new OdsResourceEndpointUrlBuilder(new CommandArgs(_configuration));
            var url = odsResourceEndpointUrlBuilder.GetOdsResourceEndpointUrl("http://www.otherserver.com", "/oneendpoint");

            url.ShouldBe("http://www.otherserver.com/Tenant1/oneendpoint?offset=0&limit=0&totalCount=true");
        }

        [Test]
        public void should_return_auth_url_with_tenant()
        {
            var odsResourceEndpointUrlBuilder = new OdsResourceEndpointUrlBuilder(new CommandArgs(_configuration));
            var url = odsResourceEndpointUrlBuilder.GetOdsAuthEndpointUrl("http://www.otherserver.com", "/oneendpoint");

            url.ShouldBe("http://www.otherserver.com/Tenant1/oneendpoint");
        }
    }
}
