// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Net;
using System.Net.Http.Headers;
using EdFi.AdminConsole.HealthCheckService.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace EdFi.AdminConsole.HealthCheckService.Features.AdminApi;

public interface IAdminApiClient
{
    Task<ApiResponse> AdminApiGet(string getInfo);

    Task<ApiResponse> AdminApiPost(StringContent content, string getInfo);
}

public class AdminApiClient : IAdminApiClient
{
    private readonly IAppHttpClient _appHttpClient;
    protected readonly ILogger _logger;
    private readonly IAdminApiSettings _adminApiOptions;
    private readonly ICommandArgs _commandArgs;
    private string _accessToken;

    public AdminApiClient(IAppHttpClient appHttpClient, ILogger logger, IOptions<AdminApiSettings> adminApiOptions, ICommandArgs commandArgs)
    {
        _appHttpClient = appHttpClient;
        _logger = logger;
        _adminApiOptions = adminApiOptions.Value;
        _commandArgs = commandArgs;
        _accessToken = string.Empty;
    }

    public async Task<ApiResponse> AdminApiGet(string getInfo)
    {
        ApiResponse response = new ApiResponse(HttpStatusCode.InternalServerError, string.Empty);
        await GetAccessToken();

        if (!string.IsNullOrEmpty(_accessToken))
        {
            const int RetryAttempts = 3;
            var currentAttempt = 0;

            while (RetryAttempts > currentAttempt)
            {
                var tenantHeader = GetTenantHeaderIfMultitenant();
                if (tenantHeader != null)
                {
                    response = await _appHttpClient.SendAsync(_adminApiOptions.ApiUrl + _adminApiOptions.AdminConsoleInstancesURI, HttpMethod.Get, tenantHeader, new AuthenticationHeaderValue("bearer", _accessToken));
                }
                else
                {
                    response = await _appHttpClient.SendAsync(_adminApiOptions.ApiUrl + _adminApiOptions.AdminConsoleInstancesURI, HttpMethod.Get, new AuthenticationHeaderValue("bearer", _accessToken));
                }

                currentAttempt++;

                if (response.StatusCode == HttpStatusCode.OK)
                    break;
            }
        }

        return response;
    }

    public async Task<ApiResponse> AdminApiPost(StringContent content, string getInfo)
    {
        ApiResponse response = new ApiResponse(HttpStatusCode.InternalServerError, string.Empty);
        await GetAccessToken();

        const int RetryAttempts = 3;
        var currentAttempt = 0;
        while (RetryAttempts > currentAttempt)
        {
            var strContent = content;
            strContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var tenantHeader = GetTenantHeaderIfMultitenant();
            if (tenantHeader != null)
                strContent.Headers.Add("tenant", tenantHeader);

             response = await _appHttpClient.SendAsync(_adminApiOptions.ApiUrl, HttpMethod.Post, content, new AuthenticationHeaderValue("bearer", _accessToken));

            currentAttempt++;

            if (response.StatusCode == HttpStatusCode.OK)
                break;
        }

        return response;
    }

    protected async Task GetAccessToken()
    {
        if (string.IsNullOrEmpty(_accessToken))
        {
            FormUrlEncodedContent contentParams = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", _commandArgs.ClientId),
                new KeyValuePair<string, string>("client_secret", _commandArgs.ClientSecret),
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("scope", "edfi_admin_api/full_access")
            });

            contentParams.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var tenantHeader = GetTenantHeaderIfMultitenant();
            if (tenantHeader != null)
                contentParams.Headers.Add("tenant", tenantHeader);

            var apiResponse = await _appHttpClient.SendAsync(_adminApiOptions.AccessTokenUrl, HttpMethod.Post, contentParams, null);

            if (apiResponse.StatusCode == HttpStatusCode.OK)
            {
                var jsonToken = JToken.Parse(apiResponse.Content);
                _accessToken = jsonToken["access_token"].ToString();
            }
            else
            {
                _logger.LogError("Not able to get Admin Api Access Token");
            }
        }
    }

    protected string? GetTenantHeaderIfMultitenant()
    {
        if (_commandArgs.IsMultiTenant)
            return _commandArgs.Tenant;

        return null;
    }
}
