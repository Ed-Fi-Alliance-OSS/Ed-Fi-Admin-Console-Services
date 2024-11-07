// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace EdFi.Ods.AdminApi.AdminConsole.HealthCheckService.Infrastructure.Services.AdminApi;

public interface IAdminApiClient
{
    Task<ApiResponse> Get(string endpointUrl, string getInfo = null);
    Task<ApiResponse> Post(string content, string endpointUrl, string postInfo = null);
}

public class AdminApiClient : IAdminApiClient
{
    private static HttpClient? _unauthenticatedHttpClient;
    private readonly ILogger _logger;
    private readonly IOptions<AppSettings> _options;
    private readonly IOptions<AdminApiSettings> _adminApiOptions;

    private Lazy<HttpClient> AuthenticatedHttpClient { get; set; }

    private string? AccessToken { get; set; }

    public AdminApiClient(ILogger logger, IOptions<AppSettings> options, IOptions<AdminApiSettings> adminApiOptions)
    {
        _logger = logger;
        _options = options;
        _adminApiOptions = adminApiOptions;
        AuthenticatedHttpClient = new Lazy<HttpClient>(CreateAuthenticatedHttpClient);

        if (_options.Value.IgnoresCertificateErrors)
        {
            _unauthenticatedHttpClient = new HttpClient(IgnoresCertificateErrorsHandler());
        }
        else
        {
            _unauthenticatedHttpClient = new HttpClient();
        }
    }

    private HttpClient CreateAuthenticatedHttpClient()
    {
        if (AccessToken == null)
            throw new Exception("An attempt was made to make authenticated HTTP requests without an Access Token.");

        HttpClient httpClient;
        if (_options.Value.IgnoresCertificateErrors)
        {
            httpClient = new HttpClient(IgnoresCertificateErrorsHandler());
        }
        else
        {
            httpClient = new HttpClient();
        }

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", AccessToken);
        return httpClient;
    }

    private async Task Authenticate()
    {
        if (AccessToken == null)
        {
            AccessToken = await GetAccessToken(_adminApiOptions.Value.AccessTokenUrl, _adminApiOptions.Value.ClientId, _adminApiOptions.Value.ClientSecret);
        }
    }

    private async Task<string> GetAuthorizationCode(string authorizeUrl, string clientId)
    {
        var contentParams = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Client_id", clientId),
                new KeyValuePair<string, string>("Response_type", "code")
            });

        _logger.LogInformation("Retrieving auth code from {url}", authorizeUrl);

        var response = await _unauthenticatedHttpClient.PostAsync(authorizeUrl, contentParams);

        if (response.StatusCode != HttpStatusCode.OK)
            throw new Exception("Failed to get Authorization Code. HTTP Status Code: " + response.StatusCode);

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var jsonToken = JToken.Parse(jsonResponse);
        return jsonToken["code"].ToString();
    }

    private static async Task<string> GetAccessToken(string accessTokenUrl, string clientId, string clientSecret, string authorizationCode = null)
    {
        FormUrlEncodedContent contentParams;

        //if (authorizationCode != null)
        //{
            contentParams = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("scope", "edfi_admin_api/full_access")
                });

        contentParams.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
        //}
        //else
        //{
        //    contentParams = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
        //        {
        //            new KeyValuePair<string, string>("Grant_type", "client_credentials")
        //        });

        //    var encodedKeySecret = Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}");
        //    _unauthenticatedHttpClient.DefaultRequestHeaders.Authorization =
        //        new AuthenticationHeaderValue("Basic", Convert.ToBase64String(encodedKeySecret));
        //}

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
        HttpResponseMessage response = null;

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

    public async Task<ApiResponse> Post(string content, string endpointUrl, string postInfo)
    {
        await Authenticate();

        const int RetryAttempts = 3;
        var currentAttempt = 0;
        HttpResponseMessage response = null;

        while (RetryAttempts > currentAttempt)
        {
            var strContent = new StringContent(content);
            strContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            response = await AuthenticatedHttpClient.Value.PostAsync(endpointUrl, strContent);
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

    private HttpClientHandler IgnoresCertificateErrorsHandler()
    {
        var handler = new HttpClientHandler();
        handler.ClientCertificateOptions = ClientCertificateOption.Manual;
        handler.ServerCertificateCustomValidationCallback =
            (httpRequestMessage, cert, cetChain, policyErrors) =>
            {
                return true;
            };

        return handler;
    }
}
