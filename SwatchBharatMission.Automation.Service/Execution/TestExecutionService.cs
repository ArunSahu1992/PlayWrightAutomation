using Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Execution
{
    /// <summary>
    /// Executes a single test case with retry, timeout and logging.
    /// This class contains ALL execution logic and is reused everywhere.
    /// </summary>
    public sealed class TestExecutionService
    {
        private readonly IOptions<AutomationSettings> _options;
        private readonly IConfigurationRegistry _registry;
        private readonly ILogger _logger;


        public TestExecutionService(
        IOptions<AutomationSettings> options,
        IConfigurationRegistry registry,
         ILoggerFactory loggerFactory)
        {
            _options = options;
            _registry = registry;
            _logger = loggerFactory.CreateLogger("TestExecutor");
        }


        public async Task<TestCaseResult> ExecuteAsync(string city, string testCaseId)
        {
            var result = new TestCaseResult
            {
                TestCaseId = testCaseId,
                StartTime = DateTime.UtcNow
            };

            try
            {
                var fileData = _registry.GetByTenant(city);
                var testInstance = TestCaseFactory.CreateInstance(testCaseId);


                if (testInstance == null)
                    throw new InvalidOperationException("Test instance not found");

                using var cts = new CancellationTokenSource(
                TimeSpan.FromMinutes(15));

                await RetryHelper.ExecuteWithRetryAsync(
                async () => await testInstance.ExecuteAsync(
                fileData,
                _options.Value,
                _logger),
                _options.Value.RetryCount,
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
