using EdFi.Ods.AdminApi.AdminConsole.HealthCheckService.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;

namespace EdFi.Ods.AdminApi.AdminConsole.HealthCheckService.Features.OdsApi;

public interface IOdsApiClient
{
    Task<ApiResponse> Get(string authenticationUrl, string clientId, string clientSecret, string resourcesUrl, string getInfo);
}

public class OdsApiClient : ApiClient, IOdsApiClient
{
    public OdsApiClient(ILogger logger, IOptions<AppSettings> options): base(logger, options)
    {
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
}
