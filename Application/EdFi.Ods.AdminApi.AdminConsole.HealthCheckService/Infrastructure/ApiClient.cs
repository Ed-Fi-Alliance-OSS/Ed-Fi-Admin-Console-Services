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

namespace EdFi.Ods.AdminApi.AdminConsole.HealthCheckService.Infrastructure;

public abstract class ApiClient
{
    protected static HttpClient _unauthenticatedHttpClient = new();
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

    protected static async Task<string> GetAccessToken(string accessTokenUrl, string clientId, string clientSecret)
    {
        FormUrlEncodedContent contentParams = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Grant_type", "client_credentials")
            });

        var encodedKeySecret = Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}");
        _unauthenticatedHttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", Convert.ToBase64String(encodedKeySecret));

        var response = await _unauthenticatedHttpClient.PostAsync(accessTokenUrl, contentParams);

        if (response.StatusCode != HttpStatusCode.OK)
            throw new Exception("Failed to get Access Token. HTTP Status Code: " + response.StatusCode);

        var jsonResult = await response.Content.ReadAsStringAsync();
        var jsonToken = JToken.Parse(jsonResult);
        return jsonToken["access_token"].ToString();
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
