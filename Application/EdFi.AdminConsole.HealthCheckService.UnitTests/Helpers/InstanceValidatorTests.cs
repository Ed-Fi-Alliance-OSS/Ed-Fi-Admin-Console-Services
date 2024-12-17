// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.AdminConsole.HealthCheckService.Features.AdminApi;
using EdFi.AdminConsole.HealthCheckService.Helpers;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;

namespace EdFi.Ods.AdminApi.HealthCheckService.UnitTests.Helpers;

public class Given_an_instance_returned_from_AdminApi
{
    private AdminApiInstanceDocument _instance = new AdminApiInstanceDocument();
    private ILogger<Given_an_instance_returned_from_AdminApi> _logger;

    [SetUp]
    public void SetUp()
    {
        _logger = A.Fake<ILogger<Given_an_instance_returned_from_AdminApi>>();

        _instance.AuthenticationUrl = "Some url";
        _instance.BaseUrl = "Some url";
        _instance.ResourcesUrl = "Some url";
        _instance.ClientId = "Some url";
        _instance.ClientSecret = "Some url";
        _instance.InstanceId = 1;
        _instance.TenantId = 1;
        _instance.InstanceName = "Some url";
    }

    [TestFixture]
    public class When_it_has_all_required_fields : Given_an_instance_returned_from_AdminApi
    {
        [Test]
        public void should_be_valid()
        {
            InstanceValidator.IsInstanceValid(_logger, _instance).ShouldBeTrue();
        }
    }

    [TestFixture]
    public class When_it_does_not_have_BaseUrl : Given_an_instance_returned_from_AdminApi
    {
        [Test]
        public void should_be_invalid()
        {
            _instance.BaseUrl = string.Empty;
            InstanceValidator.IsInstanceValid(_logger, _instance).ShouldBeFalse();
        }
    }

    [TestFixture]
    public class When_it_does_not_have_AuthenticationUrl : Given_an_instance_returned_from_AdminApi
    {
        [Test]
        public void should_be_invalid()
        {
            _instance.AuthenticationUrl = string.Empty;
            InstanceValidator.IsInstanceValid(_logger, _instance).ShouldBeFalse();
        }
    }

    [TestFixture]
    public class When_it_does_not_have_ResourcesUrl : Given_an_instance_returned_from_AdminApi
    {

        [Test]
        public void should_be_invalid()
        {
            _instance.ResourcesUrl = string.Empty;
            InstanceValidator.IsInstanceValid(_logger, _instance).ShouldBeFalse();
        }
    }

    [TestFixture]
    public class When_it_does_not_have_ClientId : Given_an_instance_returned_from_AdminApi
    {
        [Test]
        public void should_be_invalid()
        {
            _instance.ClientId = string.Empty;
            InstanceValidator.IsInstanceValid(_logger, _instance).ShouldBeFalse();
        }
    }

    [TestFixture]
    public class When_it_does_not_have_ClientSecret : Given_an_instance_returned_from_AdminApi
    {

        [Test]
        public void should_be_invalid()
        {
            _instance.ClientSecret = string.Empty;
            InstanceValidator.IsInstanceValid(_logger, _instance).ShouldBeFalse();
        }
    }

}
