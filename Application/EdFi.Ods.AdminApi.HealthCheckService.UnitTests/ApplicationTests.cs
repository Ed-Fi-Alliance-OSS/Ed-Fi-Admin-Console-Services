// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.Ods.AdminApi.AdminConsole.HealthCheckService;
using EdFi.Ods.AdminApi.HealthCheckService.UnitTests.Features.AdminApi;
using EdFi.Ods.AdminApi.HealthCheckService.UnitTests.Features.OdsApi;
using NUnit.Framework;

namespace EdFi.Ods.AdminApi.HealthCheckService.UnitTests;

[TestFixture]
public class ApplicationTests
{
    private Application _application;
    private AdminApiCallerFake _adminApiCaller;
    private OdsApiCallerFake _odsApiCaller;

    [SetUp]
    public void SetUp()
    {
        _adminApiCaller = new AdminApiCallerFake();
        _odsApiCaller = new OdsApiCallerFake();

        _application = new Application(null, _adminApiCaller, _odsApiCaller);
    }

    [Test]
    public async Task GivenACallToOdsApi_ShouldReturnHealthCheckData()
    {
        await _application.Run();
    }
}
