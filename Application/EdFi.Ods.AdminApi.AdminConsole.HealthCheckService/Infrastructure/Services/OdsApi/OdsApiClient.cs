using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace EdFi.Ods.AdminApi.AdminConsole.HealthCheckService.Infrastructure.Services.OdsApi;

public interface IOdsApiClient
{
    Task<ApiResponse> Get(string authenticationUrl, string clientId, string clientSecret, string resourcesUrl, string getInfo);
}

public class OdsApiClient : IOdsApiClient
{
    private static HttpClient _unauthenticatedHttpClient = new();
    private readonly ILogger _logger;
    private readonly IOptions<AppSettings> _options;

    private Lazy<HttpClient> AuthenticatedHttpClient { get; set; }

    private string? AccessToken { get; set; }

    public OdsApiClient(ILogger logger, IOptions<AppSettings> options)
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

    private async Task Authenticate(string authenticationUrl, string clientId, string clientSecret)
    {
        if (AccessToken == null)
        {
            AccessToken = await GetAccessToken(authenticationUrl, clientId, clientSecret);
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

    private static async Task<string> GetAccessToken(string accessTokenUrl, string clientId, string clientSecret, string? authorizationCode = null)
    {
        FormUrlEncodedContent contentParams;

        if (authorizationCode != null)
        {
            contentParams = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Client_id", clientId),
                    new KeyValuePair<string, string>("Client_secret", clientSecret),
                    new KeyValuePair<string, string>("Code", authorizationCode),
                    new KeyValuePair<string, string>("Grant_type", "authorization_code")
                });
        }
        else
        {
            contentParams = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                });

            var encodedKeySecret = Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}");
            _unauthenticatedHttpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(encodedKeySecret));
        }

        var response = await _unauthenticatedHttpClient.PostAsync(accessTokenUrl, contentParams);

        if (response.StatusCode != HttpStatusCode.OK)
            throw new Exception("Failed to get Access Token. HTTP Status Code: " + response.StatusCode);

        var jsonResult = await response.Content.ReadAsStringAsync();
        var jsonToken = JToken.Parse(jsonResult);
        return jsonToken["access_token"].ToString();
    }

    public async Task<ApiResponse> Get(string authenticationUrl, string clientId, string clientSecret, string resourcesUrl, string getInfo)
    {
        await Authenticate(authenticationUrl, clientId, clientSecret);

        const int RetryAttempts = 3;
        var currentAttempt = 0;
        HttpResponseMessage response = new HttpResponseMessage();

        while (RetryAttempts > currentAttempt)
        {
            var strContent = new StringContent(string.Empty);
            strContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            response = await AuthenticatedHttpClient.Value.GetAsync(resourcesUrl);
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
