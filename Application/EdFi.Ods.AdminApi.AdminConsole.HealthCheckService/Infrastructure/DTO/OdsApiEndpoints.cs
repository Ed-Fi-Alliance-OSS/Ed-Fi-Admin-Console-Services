using System.Collections;

namespace EdFi.Ods.AdminApi.AdminConsole.HealthCheckService.Infrastructure.DTO;

public interface IOdsApiEndpoints : IEnumerable<string>
{
    
}

public  class OdsApiEndpoints : IOdsApiEndpoints
{
    private List<string> endpoints;
    private OdsApiSettings _odsApiOptions;

    public OdsApiEndpoints(OdsApiSettings odsApiOptions)
    {
        _odsApiOptions = odsApiOptions;

        endpoints = new List<string>();
        endpoints.AddRange(_odsApiOptions.Endpoints);
    }

    public IEnumerator<string> GetEnumerator()
    {
        return endpoints.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
}
