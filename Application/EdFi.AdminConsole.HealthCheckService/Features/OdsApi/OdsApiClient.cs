// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.AdminConsole.HealthCheckService.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace EdFi.AdminConsole.HealthCheckService.Features.OdsApi;

public interface IOdsApiClient
{
    Task<ApiResponse> Get(string authenticationUrl, string clientId, string clientSecret, string resourcesUrl, string getInfo);
}

public class OdsApiClient : ApiClient, IOdsApiClient
{
    public OdsApiClient(ILogger logger, IOptions<AppSettings> options): base(logger, options)
    {
    }

    protected override async Task<string> GetAccessToken(string accessTokenUrl, string clientId, string clientSecret)
    {
        FormUrlEncodedContent contentParams;

        contentParams = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
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
}
