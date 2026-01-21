namespace Configuration
{
    public class UploadAutomationSettings
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string State { get; set; }
        public string BaseUrl { get; set; }
        public string LoginEndpoint { get; set; }
        public string TenantCode { get; set; }
        public string TestCaseName { get; set; }

        public Dictionary<string, UploadTestCase> TestCases { get; set; }
    }

    public class UploadTestCase
    {
        public string UploadApiUrl { get; set; }
    }

    public class AutomationSettings
    {
        public string Cron { get; set; }
        public string BaseUrl { get; set; }
        public BaseSetting BaseSetting { get; set; }
        public string TestCaseId { get; set; }
        public string TenantCode { get; set; }
        public string LoginEndpoint { get; set; }
        public int RetryCount { get; set; }
        public bool Headless { get; set; }
    }

    public class BaseSetting
    {
        public string State { get; set; }
        public List<TestCaseData> Data { get; set; }
    }
    public class TestCaseData
    {
        public string City { get; set; }
    }

}
