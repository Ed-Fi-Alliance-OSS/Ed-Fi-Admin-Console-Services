// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.AdminConsole.HealthCheckService.Features.OdsApi;
using EdFi.Ods.AdminApi.HealthCheckService.UnitTests.Helpers;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;

namespace EdFi.Ods.AdminApi.HealthCheckService.UnitTests.Features.OdsApi;

[TestFixture]
public class OdsApiCallerTests
{
    private ILogger<InstanceValidatorTests> _logger;
    private IOdsApiClient _fakeOdsApiClient;
    private OdsApiCaller _odsApiCaller;

    [SetUp]
    public void SetUp()
    {
        _logger = A.Fake<ILogger<InstanceValidatorTests>>();
        _fakeOdsApiClient = new OdsApiClientFake();
        _odsApiCaller = new OdsApiCaller(_logger, _fakeOdsApiClient, new AppSettingsOdsApiEndpoints(Testing.GetOdsApiSettings()));

    }

    [Test]
    public async Task GivenACallToOdsApi_ShouldReturnHealthCheckData()
    {
        var healthCheckData = await _odsApiCaller.GetHealthCheckDataAsync(Testing.AdminApiInstances.First());
        healthCheckData.ShouldBeEquivalentTo(healthCheckData);
    }
}
