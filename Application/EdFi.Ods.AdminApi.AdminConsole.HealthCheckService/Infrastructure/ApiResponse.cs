using System.Net;

namespace EdFi.Ods.AdminApi.AdminConsole.HealthCheckService.Infrastructure;
public class ApiResponse
{
    public HttpStatusCode StatusCode { get; }
    public string Content { get; }

    public ApiResponse(HttpStatusCode statusCode, string content)
    {
        StatusCode = statusCode;
        Content = content;
    }
}