using Configuration;
using Microsoft.Extensions.Options;

namespace Execution.Runner
{
    /// <summary>
    /// Executes all test cases for all configured cities.
    /// </summary>
    public sealed class FullSuiteRunner
    {
        private readonly TestExecutionService _executor;
        private readonly AutomationSettings _settings;
        private readonly IConfigurationRegistry _registry;


        public FullSuiteRunner(
        TestExecutionService executor,
        IOptions<AutomationSettings> options,
        IConfigurationRegistry registry)
        {
            _executor = executor;
            _settings = options.Value;
            _registry = registry;
        }


        public async Task<List<TestCaseResult>> RunAsync(
        List<FailedTestCase> failedTestCases)
        {
            var results = new List<TestCaseResult>();

            foreach (var city in _settings.BaseSetting.Data)
            {
                var fileData = _registry.GetByTenant(city.City);
                if (fileData?.TestCases == null) continue;


                foreach (var test in fileData.TestCases.Keys)
                {
                    var result = await _executor.ExecuteAsync(city.City, test);
                    results.Add(result);


                    if (!result.IsPassed)
                    {
                        failedTestCases.Add(new FailedTestCase
                        {
                            City = city.City,
                            TestName = test
                        });
                    }
                }
            }


            return results;
        }
    }

}
