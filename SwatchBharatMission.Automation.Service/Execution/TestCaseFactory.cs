using Automation.Core;
using TestCases;

namespace Execution
{
    public static class TestCaseFactory
    {
        private static readonly Dictionary<string, Func<ITestCase>> _testCases =
            new()
            {
            { "TC_001", () => new TC_001_UploadCityDataToServer() },
            };

        public static ITestCase Get(string testCaseId)
        {
            if (!_testCases.ContainsKey(testCaseId))
                throw new Exception($"TestCase not found: {testCaseId}");

            return _testCases[testCaseId]();
        }
    }
}
