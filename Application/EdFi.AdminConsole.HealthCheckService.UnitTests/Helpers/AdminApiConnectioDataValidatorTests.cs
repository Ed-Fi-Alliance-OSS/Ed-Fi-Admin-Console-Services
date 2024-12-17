// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Castle.Core.Configuration;
using EdFi.AdminConsole.HealthCheckService.Features;
using EdFi.AdminConsole.HealthCheckService.Helpers;
using EdFi.Ods.AdminApi.HealthCheckService.UnitTests;
using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;

namespace EdFi.AdminConsole.HealthCheckService.UnitTests.Helpers;

public class Given_AdminApiSettings_provided
{
    private Microsoft.Extensions.Configuration.IConfiguration _configuration;
    private AdminApiSettings _adminApiSettings = new AdminApiSettings();
    private ILogger<Given_AdminApiSettings_provided> _logger;
    private ICommandArgs _commandArgs;

    [SetUp]
    public void SetUp()
    {
        _logger = A.Fake<ILogger<Given_AdminApiSettings_provided>>();

        _configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(Testing.CommandArgsDicWithSingletenant)
        .Build();

        _commandArgs = new CommandArgs(_configuration);

        _adminApiSettings.ApiUrl = "http://www.myserver.com";
        _adminApiSettings.AccessTokenUrl = "http://www.myserver.com/token";
        _adminApiSettings.AdminConsoleInstancesURI = "/adminconsole/instances";
        _adminApiSettings.AdminConsoleHealthCheckURI = "/adminconsole/healthcheck";
    }

    [TestFixture]
    public class When_it_has_all_required_fields : Given_AdminApiSettings_provided
    {
        [Test]
        public void should_be_valid()
        {
            AdminApiConnectioDataValidator.IsValid(_logger, _adminApiSettings, _commandArgs).ShouldBeTrue();
        }
    }

    [TestFixture]
    public class When_it_does_not_have_ApiUrl : Given_AdminApiSettings_provided
    {
        [Test]
        public void should_be_invalid()
        {
            _adminApiSettings.ApiUrl = string.Empty;
            AdminApiConnectioDataValidator.IsValid(_logger, _adminApiSettings, _commandArgs).ShouldBeFalse();
        }
    }

    [TestFixture]
    public class When_it_does_not_have_AccessTokenUrl : Given_AdminApiSettings_provided
    {
        [Test]
        public void should_be_invalid()
        {
            _adminApiSettings.AccessTokenUrl = string.Empty;
            AdminApiConnectioDataValidator.IsValid(_logger, _adminApiSettings, _commandArgs).ShouldBeFalse();
        }
    }

    [TestFixture]
    public class When_it_does_not_have_InstancesURI : Given_AdminApiSettings_provided
    {
        [Test]
        public void should_be_invalid()
        {
            _adminApiSettings.AdminConsoleInstancesURI = string.Empty;
            AdminApiConnectioDataValidator.IsValid(_logger, _adminApiSettings, _commandArgs).ShouldBeFalse();
        }
    }

    [TestFixture]
    public class When_it_does_not_have_HealthCheckURI : Given_AdminApiSettings_provided
    {
        [Test]
        public void should_be_invalid()
        {
            _adminApiSettings.AdminConsoleHealthCheckURI = string.Empty;
            AdminApiConnectioDataValidator.IsValid(_logger, _adminApiSettings, _commandArgs).ShouldBeFalse();
        }
    }
}

public class Given_a_set_of_commans_arguments_provided
{
    public class When_it_is_a_single_tenant : Given_a_set_of_commans_arguments_provided
    {
        private Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private CommandArgs _commandArgs;
        private ILogger<Given_AdminApiSettings_provided> _logger;
        private IAdminApiSettings _adminApiSettings;

        [SetUp]
        public void SetUp()
        {
            _logger = A.Fake<ILogger<Given_AdminApiSettings_provided>>();
            _adminApiSettings = Testing.GetAdminApiSettings().Value;
        }

        [TestFixture]
        public class When_it_has_all_required_fields : When_it_is_a_single_tenant
        {
            [SetUp]
            public void SetUp()
            {
                _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(Testing.CommandArgsDicWithSingletenant)
                .Build();

                _commandArgs = new CommandArgs(_configuration);
            }

            [Test]
            public void should_be_valid()
            {
                AdminApiConnectioDataValidator.IsValid(_logger, _adminApiSettings, _commandArgs).ShouldBeTrue();
            }
        }

        [TestFixture]
        public class When_it_does_not_have_client_id : When_it_is_a_single_tenant
        {
            [SetUp]
            public void SetUp()
            {
                _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(Testing.CommandArgsDicNoClientId)
                .Build();

                _commandArgs = new CommandArgs(_configuration);
            }

            [Test]
            public void should_be_valid()
            {
                AdminApiConnectioDataValidator.IsValid(_logger, _adminApiSettings, _commandArgs).ShouldBeFalse();
            }
        }

        [TestFixture]
        public class When_it_does_not_have_client_secret : When_it_is_a_single_tenant
        {
            [SetUp]
            public void SetUp()
            {
                _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(Testing.CommandArgsDicNoClientSecret)
                .Build();

                _commandArgs = new CommandArgs(_configuration);
            }

            [Test]
            public void should_be_valid()
            {
                AdminApiConnectioDataValidator.IsValid(_logger, _adminApiSettings, _commandArgs).ShouldBeFalse();
            }
        }
    }
    public class When_it_is_a_multi_tenant : Given_a_set_of_commans_arguments_provided
    {
        private Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private CommandArgs _commandArgs;
        private ILogger<Given_AdminApiSettings_provided> _logger;
        private IAdminApiSettings _adminApiSettings;

        [SetUp]
        public void SetUp()
        {
            _logger = A.Fake<ILogger<Given_AdminApiSettings_provided>>();
            _adminApiSettings = Testing.GetAdminApiSettings().Value;
        }

        [TestFixture]
        public class When_it_has_all_required_fields : When_it_is_a_multi_tenant
        {
            [SetUp]
            public void SetUp()
            {
                _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(Testing.CommandArgsDicWithMultitenant)
                .Build();

                _commandArgs = new CommandArgs(_configuration);
            }

            [Test]
            public void should_be_valid()
            {
                AdminApiConnectioDataValidator.IsValid(_logger, _adminApiSettings, _commandArgs).ShouldBeTrue();
            }
        }

        [TestFixture]
        public class When_it_does_not_have_tenant : When_it_is_a_multi_tenant
        {
            [SetUp]
            public void SetUp()
            {
                _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(Testing.CommandArgsDicWithMultitenantNoTenant)
                .Build();

                _commandArgs = new CommandArgs(_configuration);
            }

            [Test]
            public void should_be_invalid()
            {
                AdminApiConnectioDataValidator.IsValid(_logger, _adminApiSettings, _commandArgs).ShouldBeFalse();
            }
        }
    }
}
