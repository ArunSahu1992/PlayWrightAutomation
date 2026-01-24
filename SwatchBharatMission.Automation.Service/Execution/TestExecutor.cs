using Automation.Core;
using Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Serilog.Core;

namespace Execution
{
    public class TestExecutor
    {
        private readonly IOptions<AutomationSettings> _options;
        private string _testCaseId;
        private readonly ILogger _logger;
        private readonly IConfigurationRegistry _registry;
        public TestExecutor(IOptions<AutomationSettings> options, ILoggerFactory loggerFactory, IConfigurationRegistry registry)
        {
            _options = options;
            _registry = registry;
            _logger = loggerFactory.CreateLogger("TestExecutor");
        }
        public async Task ExecuteAsync()
        {
            List<TestCaseResult> testCases = new();
            List<FailedTestCase> failedTestCases = new();

            try
            {
                var retryTests = FailedTestCaseConfiguration.ReadTodayFailedTests();
                if (retryTests.Any())
                {
                    Console.WriteLine("Retrying failed test cases...");
                    foreach (var testCase in retryTests)
                    {
                        var result = new TestCaseResult
                        {
                            TestCaseId = testCase.TestName,
                            StartTime = DateTime.UtcNow
                        };
                        try
                        {
                            var testInstance = TestCaseFactory.CreateInstance(testCase.TestName);
                            if (testInstance is null) continue;

                            var fileData = _registry.GetByTenant(testCase.City);

                            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
                            await RetryHelper.ExecuteWithRetryAsync(
                            async () =>
                            {
                                await testInstance.ExecuteAsync(
                                    fileData,
                                    _options.Value,
                                    _logger
                                );
                            },
                            retryCount: _options.Value.RetryCount,
                            token: cts.Token,
                            _logger: _logger
                        );
                            result.IsPassed = true;
                            result.Message = "Success";
                            retryTests.Remove(testCase);
                        }
                        catch (Exception ex)
                        {
                            result.IsPassed = false;
                            result.Message = "Failed";
                            _logger.LogInformation($"Exception Occurred for TestCase {testCase.TestName} " + ex.Message);
                        }
                        finally
                        {
                            FailedTestCaseConfiguration.WriteFailedTests(retryTests);
                            testCases.Add(result);
                            result.EndTime = DateTime.UtcNow;
                        }

                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(_options.Value.TestCaseId))
                    {
                        // Get all the json file name from appsetting json.
                        // take each json read it  and execute all testcase for the same file.

                        var baseSettings = _options.Value.BaseSetting;
                        if (baseSettings != null)
                        {
                            var cityNames = baseSettings.Data;

                            if (cityNames != null && cityNames.Count > 0)
                            {
                                foreach (var item in cityNames)
                                {
                                    var fileData = _registry.GetByTenant(item.City);

                                    if (fileData != null && fileData.TestCases is { Count: > 0 })
                                    {
                                        foreach (var testcase in fileData.TestCases)
                                        {
                                            var result = new TestCaseResult
                                            {
                                                TestCaseId = testcase.Key,
                                                StartTime = DateTime.UtcNow
                                            };
                                            try
                                            {
                                                var testInstance = TestCaseFactory.CreateInstance(testcase.Key);
                                                if (testInstance is null) continue;

                                                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
                                                await RetryHelper.ExecuteWithRetryAsync(
                                                async () =>
                                                {
                                                    await testInstance.ExecuteAsync(
                                                        fileData,
                                                        _options.Value,
                                                        _logger
                                                    );
                                                },
                                                retryCount: _options.Value.RetryCount,
                                                token: cts.Token,
                                                _logger: _logger
                                            );
                                                result.IsPassed = true;
                                                result.Message = "Success";
                                            }
                                            catch (Exception ex)
                                            {
                                                result.IsPassed = false;
                                                result.Message = "Failed";
                                                _logger.LogInformation($"Exception Occurred for TestCase {testcase.Key} " + ex.Message);
                                                failedTestCases.Add(new FailedTestCase() { City = item.City, TestName = testcase.Key });
                                            }
                                            finally
                                            {
                                                testCases.Add(result);
                                                result.EndTime = DateTime.UtcNow;
                                            }
                                        }
                                    }
                                }

                            }
                        }
                    }
                }

            }
            finally
            {
                FailedTestCaseConfiguration.WriteFailedTests(failedTestCases);
                ReportGenerator.GenerateReport(testCases);
                Console.WriteLine();
                Console.WriteLine("┌───────────────────────────────────────────┬──────────┬──────────────────┬────────────────────┐");
                Console.WriteLine("│ Test Case ID                              │ Status   │ Start Time       │ End Time           │");
                Console.WriteLine("├───────────────────────────────────────────┼──────────┼──────────────────┼────────────────────┤");

                foreach (var item in testCases)
                {
                    Console.WriteLine(
                                 $"│ {item.TestCaseId}                         │ {item.Status}  │ {item.StartTime:yyyy-MM-dd HH:mm:ss} │ {item.EndTime:yyyy-MM-dd HH:mm:ss} │"
                    );
                }
            }
        }
    }
}
