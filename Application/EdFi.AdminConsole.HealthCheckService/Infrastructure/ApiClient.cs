// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;

namespace EdFi.AdminConsole.HealthCheckService.Infrastructure;

public abstract class ApiClient
{
    protected static HttpClient? _unauthenticatedHttpClient;
    protected readonly ILogger _logger;
    protected readonly IOptions<AppSettings> _options;

    protected Lazy<HttpClient> AuthenticatedHttpClient { get; set; }

    protected string? AccessToken { get; set; }

    public ApiClient(ILogger logger, IOptions<AppSettings> options)
    {
        _logger = logger;
        _options = options;
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

    protected HttpClient CreateAuthenticatedHttpClient()
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

    protected async Task Authenticate(string authenticationUrl, string clientId, string clientSecret)
    {
        if (AccessToken == null)
        {
            AccessToken = await GetAccessToken(authenticationUrl, clientId, clientSecret);
        }
    }

    protected virtual Task<string> GetAccessToken(string accessTokenUrl, string clientId, string clientSecret)
    {
        throw new NotImplementedException();
    }

    public async Task<ApiResponse> Get(string authenticationUrl, string clientId, string clientSecret, string odsEndpointUrl, string getInfo)
    {
        await Authenticate(authenticationUrl, clientId, clientSecret);

        const int RetryAttempts = 3;
        var currentAttempt = 0;
        HttpResponseMessage response = new HttpResponseMessage();

        while (RetryAttempts > currentAttempt)
        {
            var strContent = new StringContent(string.Empty);
            strContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            response = await AuthenticatedHttpClient.Value.GetAsync(odsEndpointUrl);
            currentAttempt++;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                AccessToken = null;
                await Authenticate(authenticationUrl, clientId, clientSecret);
                AuthenticatedHttpClient = new Lazy<HttpClient>(CreateAuthenticatedHttpClient);
                _logger.LogWarning("GET failed. Reason: {reason}. StatusCode: {status}.", response.ReasonPhrase, response.StatusCode);
                _logger.LogInformation("Refreshing token and retrying GET request for {info}.", getInfo);
            }
            else
                break;
        }

        var responseContent = await response.Content.ReadAsStringAsync();

        return new ApiResponse(response.StatusCode, responseContent, response.Headers);
    }

    public async Task<ApiResponse> Post(string authenticationUrl, string clientId, string clientSecret, StringContent content, string endpointUrl, string postInfo)
    {
        await Authenticate(authenticationUrl, clientId, clientSecret);

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
                await Authenticate(authenticationUrl, clientId, clientSecret);
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
