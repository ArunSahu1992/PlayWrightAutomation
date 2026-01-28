using Configuration;
using Execution.context;

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
                results.Add(
                await _executor.ExecuteAsync(automationContext,
                testCase.City,
                testCase.TestName));
            }

            return results;
        }
    }
}
