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

            foreach (var testCase in retryTests)
            {
                var cityKey = automationContext.Cities[testCase.City];
                automationContext.automationFlowSettings.TenantCode = testCase.TenantCode;
                results.Add(
                await _executor.ExecuteAsync(automationContext,
                testCase.City,
                testCase.TestName));
            }
            var failedTestCase = results.Where(x => x.IsPassed == false)?.Select(x => new Configuration.FailedTestCase()
            {
                TestName = x.TestCaseId,
                City = retryTests.FirstOrDefault(p => p.TestName == x.TestCaseId)?.City,
                TenantCode = retryTests.FirstOrDefault(p => p.TestName == x.TestCaseId)?.TenantCode
            });

            if (failedTestCase.Any())
                FailedTestCaseConfiguration.WriteFailedTests(failedTestCase.ToList());

            return results;
        }
    }
}
