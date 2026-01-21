using Automation.Core;
using System.Reflection;
using TestCases;

namespace Execution
{
    public static class TestCaseFactory
    {
        private static readonly Dictionary<string, Func<ITestCase>> _testCases =
            new()
            {
            { "TC_001", () => new TC_001_UploadCityDataToServer_MR_05() },
            { "TC_002", () => new TC_002_UploadCityDataToServer_MR_3A_SWM() },
            };

        public static ITestCase Get(string testCaseId)
        {
            if (!_testCases.ContainsKey(testCaseId))
                throw new Exception($"TestCase not found: {testCaseId}");

            return _testCases[testCaseId]();
        }

        public static ITestCase? CreateInstance(string className)
        {
            if (string.IsNullOrWhiteSpace(className))
                return null;

            var type = GetTestType(className);

            return type == null
                ? null
                : Activator.CreateInstance(type) as ITestCase;
        }
        public static Type? GetTestType(string className)
        {
            var basePath = AppContext.BaseDirectory;

            var assemblies = Directory.GetFiles(basePath, "*.dll")
                .Select(Assembly.LoadFrom);

            return assemblies
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t =>
                    typeof(ITestCase).IsAssignableFrom(t) &&
                    !t.IsInterface &&
                    !t.IsAbstract &&
                    t.FullName == string.Concat("TestCases.",className));
        }
    }
}
