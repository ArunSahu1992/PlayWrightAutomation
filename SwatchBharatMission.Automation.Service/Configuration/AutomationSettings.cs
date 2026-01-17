namespace Configuration
{
    public class AutomationSettings
    {
        public string Cron { get; set; }
        public string BaseUrl { get; set; }
        public string TestCaseId { get; set; }
        public string LoginEndpoint { get; set; }
        public string TenantCode { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public Dictionary<string, Dictionary<string, string>> TestCases { get; set; }
        public string City { get; set; }
        public string DownloadPath { get; set; }
        public string UploadApiUrl { get; set; }
        public bool Headless { get; set; }
    }

}
