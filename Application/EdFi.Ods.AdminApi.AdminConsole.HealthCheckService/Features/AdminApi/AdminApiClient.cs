// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.Ods.AdminApi.AdminConsole.HealthCheckService.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;

namespace EdFi.Ods.AdminApi.AdminConsole.HealthCheckService.Features.AdminApi;

public interface IAdminApiClient
{
    Task<ApiResponse> Get(string endpointUrl, string getInfo);
    Task<ApiResponse> Post(StringContent content, string endpointUrl, string postInfo);
}

public class AdminApiClient : ApiClient, IAdminApiClient
{
    private readonly IOptions<AdminApiSettings> _adminApiOptions;

    public AdminApiClient(ILogger logger, IOptions<AppSettings> options, IOptions<AdminApiSettings> adminApiOptions) : base(logger, options)
    {
        _adminApiOptions = adminApiOptions;
    }

    private async Task Authenticate()
    {
        if (AccessToken == null)
        {
            AccessToken = await GetAccessToken(_adminApiOptions.Value.AccessTokenUrl, _adminApiOptions.Value.ClientId, _adminApiOptions.Value.ClientSecret);
        }
    }

    private static new async Task<string> GetAccessToken(string accessTokenUrl, string clientId, string clientSecret)
    {
        FormUrlEncodedContent contentParams = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("scope", "edfi_admin_api/full_access")
            });

        contentParams.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

        var response = await _unauthenticatedHttpClient.PostAsync(accessTokenUrl, contentParams);

        var responseString = await response.Content.ReadAsStringAsync();

        if (response.StatusCode != HttpStatusCode.OK)
            throw new Exception("Failed to get Access Token. HTTP Status Code: " + response.StatusCode);

        var jsonResult = await response.Content.ReadAsStringAsync();
        var jsonToken = JToken.Parse(jsonResult);
        return jsonToken["access_token"].ToString();
    }

    public async Task<ApiResponse> Get(string endpointUrl, string getInfo)
    {
        await Authenticate();

        const int RetryAttempts = 3;
        var currentAttempt = 0;
        HttpResponseMessage response = new HttpResponseMessage();

        while (RetryAttempts > currentAttempt)
        {
            var strContent = new StringContent(string.Empty);
            strContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            response = await AuthenticatedHttpClient.Value.GetAsync(endpointUrl);
            currentAttempt++;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                AccessToken = null;
                await Authenticate();
                AuthenticatedHttpClient = new Lazy<HttpClient>(CreateAuthenticatedHttpClient);
                _logger.LogWarning("GET failed. Reason: {reason}. StatusCode: {status}.", response.ReasonPhrase, response.StatusCode);
                _logger.LogInformation("Refreshing token and retrying GET request for {info}.", getInfo);
            }
            else
                break;
        }

        var responseContent = await response.Content.ReadAsStringAsync();

        return new ApiResponse(response.StatusCode, responseContent);
    }

    public async Task<ApiResponse> Post(StringContent content, string endpointUrl, string postInfo)
    {
        await Authenticate();

        const int RetryAttempts = 3;
        var currentAttempt = 0;
        HttpResponseMessage response = new HttpResponseMessage();

        while (RetryAttempts > currentAttempt)
        {
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            response = await AuthenticatedHttpClient.Value.PostAsync(endpointUrl, content);
            currentAttempt++;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                AccessToken = null;
                await Authenticate();
                AuthenticatedHttpClient = new Lazy<HttpClient>(CreateAuthenticatedHttpClient);
                _logger.LogWarning("POST failed. Reason: {reason}. StatusCode: {status}.", response.ReasonPhrase, response.StatusCode);
                _logger.LogInformation("Refreshing token and retrying POST request for {info}.", postInfo);
            }
            else
                break;
        }

        var responseContent = await response.Content.ReadAsStringAsync();

        return new ApiResponse(response.StatusCode, responseContent);
    }
}
