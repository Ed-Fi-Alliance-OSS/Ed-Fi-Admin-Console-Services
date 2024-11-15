// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.AdminConsole.HealthCheckService.Features.AdminApi;
using EdFi.Ods.AdminApi.HealthCheckService.UnitTests.Helpers;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;

namespace EdFi.Ods.AdminApi.HealthCheckService.UnitTests.Features.AdminApi;

[TestFixture]
public class AdminApiCallerTests
{
    private ILogger<InstanceValidatorTests> _logger;
    private IAdminApiClient _fakeAdminApiClient;
    private AdminApiCaller _adminApiCaller;

    [SetUp]
    public void SetUp()
    {
        _logger = A.Fake<ILogger<InstanceValidatorTests>>();
        _fakeAdminApiClient = new AdminApiClientFake();
        _adminApiCaller = new AdminApiCaller(_logger, _fakeAdminApiClient, Testing.GetAdminApiSettings());
    }

    [Test]
    public async Task GivenACallToAdminApi_ShouldReturnInstances()
    {
        var instances = await _adminApiCaller.GetInstancesAsync();
        instances.Count().ShouldBe(2);

        instances.First().InstanceId.ShouldBe(1);
        instances.First().TenantId.ShouldBe(1);
        instances.First().InstanceName.ShouldBe("instance 1");
        instances.First().ClientId.ShouldBe("one client");
        instances.First().ClientSecret.ShouldBe("one secret");
        instances.First().BaseUrl.ShouldBe("one base url");
        instances.First().ResourcesUrl.ShouldBe("one resourse url");
        instances.First().AuthenticationUrl.ShouldBe("one auth url");

        instances.ElementAt(1).InstanceId.ShouldBe(2);
        instances.ElementAt(1).TenantId.ShouldBe(2);
        instances.ElementAt(1).InstanceName.ShouldBe("instance 2");
        instances.ElementAt(1).ClientId.ShouldBe("another client");
        instances.ElementAt(1).ClientSecret.ShouldBe("another secret");
        instances.ElementAt(1).BaseUrl.ShouldBe("another base url");
        instances.ElementAt(1).ResourcesUrl.ShouldBe("another resourse url");
        instances.ElementAt(1).AuthenticationUrl.ShouldBe("another auth url");
    }
}