using Configuration;
using Execution.context;
using Execution.Runner;
using Microsoft.Extensions.Logging;

namespace Execution.AutomationFlow
{
    public class MnregaAutomationFlow : IAutomationFlow
    {
        public string Name => Constants.MANREGA_FLOW_NAME;
        private readonly FullSuiteRunner _fullRunner;
        private readonly FailedTestRunner _failedRunner;
        private readonly ILogger<MnregaAutomationFlow> _logger;

        public MnregaAutomationFlow(
        FullSuiteRunner fullRunner,
        FailedTestRunner failedRunner,
        ILogger<MnregaAutomationFlow> logger)
        {
            _fullRunner = fullRunner;
            _failedRunner = failedRunner;
            _logger = logger;
        }

        public async Task<List<TestCaseResult>> ExecuteAsync(AutomationContext automationContext)
        {
            var testResults = new List<TestCaseResult>();
            var failedTestCases = new List<FailedTestCase>();
            var retryTests = FailedTestCaseConfiguration.ReadTodayFailedTests();


            if (retryTests.Any())
            {
                testResults.AddRange(
                await _failedRunner.RunAsync(retryTests, automationContext));
            }
            else
            {
                testResults.AddRange(
                await _fullRunner.RunAsync(failedTestCases, automationContext));
            }

            FailedTestCaseConfiguration.WriteFailedTests(failedTestCases);
            return testResults;
        }
    }
}
