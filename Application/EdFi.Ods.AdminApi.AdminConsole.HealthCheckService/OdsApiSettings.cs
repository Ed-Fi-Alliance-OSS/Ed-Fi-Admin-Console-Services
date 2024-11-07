namespace EdFi.Ods.AdminApi.AdminConsole.HealthCheckService;

public interface IOdsApiSettings
{
    IEnumerable<string> Endpoints { get; set; }
}

public  class OdsApiSettings : IOdsApiSettings
{
    public IEnumerable<string> Endpoints { get; set; } = new List<string>();
}
