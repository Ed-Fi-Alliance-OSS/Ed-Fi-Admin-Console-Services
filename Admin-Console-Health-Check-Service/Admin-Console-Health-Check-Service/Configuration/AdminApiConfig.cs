namespace Admin_Console_Health_Check_Service.Configuration
{
    public class AdminApiConfig
    {
        public string HealthCheckUrl { get; set; }
        public string TokenUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string GrantType { get; set; }
        public string Scope { get; set; }
    }
}
