using Configuration;
using Execution.context;
using static Configuration.TestCaseResult;
using FailedTestCase = Configuration.FailedTestCase;

namespace Execution.Runner
{
    /// <summary>
    /// Executes only previously failed test cases.
    /// </summary>
    public sealed class FailedTestRunner
    {
        private readonly TestExecutionService _executor;
        public FailedTestRunner(TestExecutionService executor)
        {
            _executor = executor;
        }
        public async Task<List<TestCaseResult>> RunAsync(
        List<FailedTestCase> retryTests, AutomationContext automationContext)
        {
            var results = new List<TestCaseResult>();
            var passedOne = new List<FailedTestCase>();

            foreach (var testCase in retryTests)
            {
                var cityKey = automationContext.Cities[testCase.City];
                automationContext.automationFlowSettings.TenantCode = testCase.TenantCode;
                automationContext.automationFlowSettings.City = testCase.City;

                var result = await _executor.ExecuteAsync(automationContext,
                testCase.City,
                testCase.TestName);

                results.Add(result);

                if (result.IsPassed)
                {
                    passedOne.Add(new FailedTestCase() { TestName = testCase.TestName, TenantCode = testCase.TenantCode });
                }
            }

            // handle passed scenrio only.

            if (!passedOne.Any()) return results;

            var failedTCs = FailedTestCaseConfiguration.ReadTodayFailedTests();

            failedTCs.RemoveAll(f => passedOne.Any(p => p.TestName == f.TestName && p.TenantCode == f.TenantCode));

            FailedTestCaseConfiguration.WriteFailedTests(failedTCs);

            return results;
        }
    }
}
