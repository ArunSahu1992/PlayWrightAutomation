using Automation.Core;
using System.Reflection;
using Configuration;

namespace Execution
{
    public static class TestCaseFactory
    {

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
