using Automation.Core;
using Configuration;
using Microsoft.Extensions.Options;

namespace Execution
{
    public class TestExecutor
    {
        private readonly IOptions<AutomationSettings> _options;
        private string _testCaseId;
        public TestExecutor(IOptions<AutomationSettings> options)
        {
            _options = options;
        }
        public async Task ExecuteAsync()
        {
            List<TestCaseResult> testCases = new List<TestCaseResult>();
            try
            {
                if (string.IsNullOrEmpty(_options.Value.TestCaseId))
                {
                    foreach (var item in _options.Value.TestCases)
                    {
                        var testContext = await PlaywrightFactory.CreateAsync();
                        var testCase = TestCaseFactory.Get(item.Key);
                        var result = new TestCaseResult
                        {
                            TestCaseId = item.Key,
                            StartTime = DateTime.UtcNow
                        };
                        try
                        {
                            await testCase.ExecuteAsync(testContext, item.Value, _options);
                            result.IsPassed = true;
                            result.Message = "Success";
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Exception occured " + ex.Message);
                            result.IsPassed = false;
                            result.Message = ex.Message;
                        }
                        finally
                        {
                            testCases.Add(result);
                            result.EndTime = DateTime.UtcNow; 
                        }
                    }
                }
                else
                {
                    var testCaseKey = _options.Value.TestCases.FirstOrDefault(x => x.Key == _options.Value.TestCaseId);
                    var testContext = await PlaywrightFactory.CreateAsync();
                    var testCase = TestCaseFactory.Get(testCaseKey.Key);
                    var result = new TestCaseResult
                    {
                        TestCaseId = testCaseKey.Key,
                        StartTime = DateTime.UtcNow
                    };
                    try
                    {
                        await testCase.ExecuteAsync(testContext, testCaseKey.Value, _options);
                        result.IsPassed = true;
                        result.Message = "Success";
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception occured " + ex.Message);
                        result.IsPassed = false;
                        result.Message = ex.Message;
                    }
                    finally
                    {
                        testCases.Add(result);
                        result.EndTime = DateTime.UtcNow;
                    }
                }
            }
            finally
            {
                ReportGenerator.GenerateReport(testCases, _options.Value.DownloadPath);
                Console.WriteLine();
                Console.WriteLine("┌────────────────────┬──────────┬────────────────────┬────────────────────┐");
                Console.WriteLine("│ Test Case ID       │ Status   │ Start Time         │ End Time           │");
                Console.WriteLine("├────────────────────┼──────────┼────────────────────┼────────────────────┤");

                foreach (var item in testCases)
                {
                    Console.WriteLine(
                        $"│ {item.TestCaseId,-18} │ {item.Status,-8} │ {item.StartTime:yyyy-MM-dd HH:mm:ss} │ {item.EndTime:yyyy-MM-dd HH:mm:ss} │"
                    );
                }

            }
        }
    }
}
