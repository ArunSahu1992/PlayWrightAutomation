using Configuration;
using Execution.context;
using Execution.Runner;
using Microsoft.Extensions.Logging;

namespace Execution.AutomationFlow
{
    public class SwatchBharatAutomationFlow :IAutomationFlow
    {
        public string Name => Constants.SWATCHBHARAT_FLOW_NAME;
        private readonly FullSuiteRunner _fullRunner;
        private readonly FailedTestRunner _failedRunner;
        private readonly ILogger<MnregaAutomationFlow> _logger;

        public SwatchBharatAutomationFlow(
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

            retryTests = retryTests.Where(x => x.Flow == Name).ToList();

            if (retryTests.Any())
            {
                testResults.AddRange(
                await _failedRunner.RunAsync(retryTests, automationContext));
            }
            else if (automationContext.IsFirstRun)
            {
                testResults.AddRange(
                await _fullRunner.RunAsync(failedTestCases, automationContext));
            }
            return testResults;
        }

    }
}
