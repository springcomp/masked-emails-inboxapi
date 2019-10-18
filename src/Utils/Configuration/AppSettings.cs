namespace Utils.Configuration
{
    public class AppSettings
    {
        public string DomainName { get; set; }
        public string Authority { get; set; }
        public bool RequireHttpsMetadata { get; set; }
        public string Audience { get; set; }
        public int HttpsPort { get; set; } = 443;
    }
}