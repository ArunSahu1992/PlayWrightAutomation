using Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Execution
{
    /// <summary>
    /// Executes a single test case with retry, timeout and logging.
    /// This class contains ALL execution logic and is reused everywhere.
    /// </summary>
    public sealed class TestExecutionService
    {
        private readonly IOptions<AutomationSettings> _options;
        private readonly IEnumerable<ITestCase> _allTestCases;
        private readonly ILogger _logger;


        public TestExecutionService(
        IOptions<AutomationSettings> options, IEnumerable<ITestCase> allTestCases,
         ILoggerFactory loggerFactory)
        {
            _options = options;
            _allTestCases = allTestCases;
            _logger = loggerFactory.CreateLogger("TestExecutor");
        }


        public async Task<TestCaseResult> ExecuteAsync(AutomationContext automationContext, string city, string testCaseId)
        {
            var result = new TestCaseResult
            {
                TestCaseId = testCaseId,
                StartTime = DateTime.UtcNow
            };

            try
            {
                var testInstance  = _allTestCases.First(f =>
                f.TestCaseId.Equals(testCaseId, StringComparison.OrdinalIgnoreCase));

                if (testInstance == null)
                    throw new InvalidOperationException("Test instance not found");

                using var cts = new CancellationTokenSource(
                TimeSpan.FromMinutes(15));

                await RetryHelper.ExecuteWithRetryAsync(
                async () => await testInstance.ExecuteAsync(
                automationContext,
                _logger),
                automationContext.automationFlowSettings.RetryCount,
                cts.Token,
                _logger);


                result.IsPassed = true;
                result.Message = "Success";
            }
            catch (Exception ex)
            {
                result.IsPassed = false;
                result.Message = "Failed";
                _logger.LogError(ex,
                "Exception occurred while executing TestCase {TestCaseId}",
                testCaseId);
            }
            finally
            {
                result.EndTime = DateTime.UtcNow;
            }

            return result;
        }
    }
}
