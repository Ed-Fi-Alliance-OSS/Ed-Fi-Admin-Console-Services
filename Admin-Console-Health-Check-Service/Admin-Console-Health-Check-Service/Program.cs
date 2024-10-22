using Admin_Console_Health_Check_Service.Configuration;
using Admin_Console_Health_Check_Service.Response;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Writers;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AdminApiConfig>(
    builder.Configuration.GetSection("AdminApiConfig")
);

builder.Services.AddHttpClient();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
const string allowAllCorsPolicyName = "allowAllCorsPolicyName";
builder.Services.AddCors(options =>
{
    options.AddPolicy(allowAllCorsPolicyName, policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


var app = builder.Build();

// Usar CORS
app.UseCors(allowAllCorsPolicyName);

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.MapGet("/healthcheck", async (IOptions<AdminApiConfig> apiSettings, IHttpClientFactory httpClientFactory) =>
{
    // Get settings and create client
    var settings = apiSettings.Value;
    var client = httpClientFactory.CreateClient();
    
    // Create Token Request
    var tokenRequest = new HttpRequestMessage(HttpMethod.Post, settings.TokenUrl);
    var content = new List<KeyValuePair<string, string>>() 
    {
        new("client_id", settings.ClientId),
        new("client_secret", settings.ClientSecret),
        new("grant_type", settings.GrantType),
        new("scope", settings.Scope),
    };
    tokenRequest.Content = new FormUrlEncodedContent(content);
    
    // Request token
    var tokenResponse = await client.SendAsync(tokenRequest);
    if (!tokenResponse.IsSuccessStatusCode)
    {
        return Results.StatusCode((int)tokenResponse.StatusCode);
    }

    // Get Token
    var responseData = await tokenResponse.Content.ReadAsStringAsync();
    var tokenInfo = JsonSerializer.Deserialize<GetTokenResponse>(responseData);

    // Create Health Check Request
    var hckRequest = new HttpRequestMessage(HttpMethod.Get, settings.HealthCheckUrl);
    hckRequest.Headers.Add("Authorization", "Bearer " + tokenInfo?.AccessToken);
    
    // Request Health Check Information
    var response = await client.SendAsync(hckRequest);
    if (response.IsSuccessStatusCode)
    {
        return Results.Content(await response.Content.ReadAsStringAsync());
    }
    return Results.StatusCode((int)response.StatusCode);

});

app.Run();
