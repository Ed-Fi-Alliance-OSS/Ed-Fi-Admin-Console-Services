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

[TestFixture]
public class InstanceValidatorTests
{
    private AdminApiInstance _instance = new AdminApiInstance();
    private ILogger<InstanceValidatorTests> _logger;

    [SetUp]
    public void SetUp()
    {
        _logger = A.Fake<ILogger<InstanceValidatorTests>>();

        _instance.AuthenticationUrl = "Some url";
        _instance.BaseUrl = "Some url";
        _instance.ResourcesUrl = "Some url";
        _instance.ClientId = "Some url";
        _instance.ClientSecret = "Some url";
        _instance.InstanceId = 1;
        _instance.TenantId = 1;
        _instance.InstanceName = "Some url";
    }

    [Test]
    public void GivenAnInstanceWithoutAuthenticationUrl_NoErrorsAreReturned()
    {
        _instance.AuthenticationUrl = string.Empty;
        InstanceValidator.IsInstanceValid(_logger, _instance).ShouldBeFalse();
    }

    [Test]
    public void GivenAnInstanceWithoutAuthenticationUrl_AnErrorIsReturned()
    {
        _instance.AuthenticationUrl = string.Empty;
        InstanceValidator.IsInstanceValid(_logger, _instance).ShouldBeFalse();
    }

    [Test]
    public void GivenAnInstanceWithoutResourcesUrl_AnErrorIsReturned()
    {
        _instance.ResourcesUrl = string.Empty;
        InstanceValidator.IsInstanceValid(_logger, _instance).ShouldBeFalse();
    }

    [Test]
    public void GivenAnInstanceWithoutClientId_AnErrorIsReturned()
    {
        _instance.ClientId = string.Empty;
        InstanceValidator.IsInstanceValid(_logger, _instance).ShouldBeFalse();
    }

    [Test]
    public void GivenAnInstanceWithoutSecret_AnErrorIsReturned()
    {
        _instance.ClientSecret = string.Empty;
        InstanceValidator.IsInstanceValid(_logger, _instance).ShouldBeFalse();
    }
}
