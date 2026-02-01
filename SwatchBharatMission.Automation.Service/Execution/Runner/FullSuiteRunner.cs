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


        public FullSuiteRunner(
        TestExecutionService executor,
        IOptions<AutomationSettings> options)
        {
            _executor = executor;
            _settings = options.Value;
        }


        public async Task<List<TestCaseResult>> RunAsync(
        List<FailedTestCase> failedTestCases, AutomationContext automationContext)
        {
            var results = new List<TestCaseResult>();

            foreach (var city in automationContext.automationFlowSettings.BaseSetting.Data)
            {
                var cityKey = automationContext.Cities[city.City];
                if (cityKey is null) continue;
                foreach (var test in cityKey.TestCases)
                {
                    automationContext.automationFlowSettings.TenantCode = city.TenantCode;
                    automationContext.automationFlowSettings.City = city.City;
                    var result = await _executor.ExecuteAsync(automationContext,city.City, test.Key);
                    results.Add(result);

                    if (!result.IsPassed)
                    {
                        failedTestCases =  failedTestCases ?? new List<FailedTestCase>();
                        failedTestCases.Add(new FailedTestCase
                        {
                            City = city.City,
                            TestName = test.Key,
                            TenantCode = city.TenantCode,
                            Flow = automationContext.FlowName
                        });
                    }
                }

                var failedTCs = FailedTestCaseConfiguration.ReadTodayFailedTests();
                failedTCs.AddRange(failedTestCases);
                FailedTestCaseConfiguration.WriteFailedTests(failedTCs);
            }
            return results;
        }
    }

}
