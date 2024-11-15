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

namespace EdFi.AdminConsole.HealthCheckService.Features.AdminApi;

public interface IAdminApiClient
{
    Task<ApiResponse> Get(string authenticationUrl, string clientId, string clientSecret, string odsEndpointUrl, string getInfo);
    Task<ApiResponse> Post(string authenticationUrl, string clientId, string clientSecret, StringContent content, string endpointUrl, string postInfo);
}

public class AdminApiClient : ApiClient, IAdminApiClient
{
    private readonly IOptions<AdminApiSettings> _adminApiOptions;

    public AdminApiClient(ILogger logger, IOptions<AppSettings> options, IOptions<AdminApiSettings> adminApiOptions) : base(logger, options)
    {
        _adminApiOptions = adminApiOptions;
    }

    protected override async Task<string> GetAccessToken(string accessTokenUrl, string clientId, string clientSecret)
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
}
