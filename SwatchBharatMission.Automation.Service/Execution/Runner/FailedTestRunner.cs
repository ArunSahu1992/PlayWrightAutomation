using Configuration;

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
        List<FailedTestCase> retryTests)
        {
            var results = new List<TestCaseResult>();


            foreach (var testCase in retryTests)
            {
                results.Add(
                await _executor.ExecuteAsync(
                testCase.City,
                testCase.TestName));
            }

            return results;
        }
    }
}
