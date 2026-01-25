using Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Execution.Runner
{
    /// <summary>
    /// Orchestrates execution flow and reporting.
    /// Decides whether to run full suite or retry failed tests.
    /// </summary>
    public sealed class Orchestrator
    {
        private readonly FullSuiteRunner _fullSuite;
        private readonly FailedTestRunner _failedRunner;

        public Orchestrator(
        FullSuiteRunner fullSuite,
        FailedTestRunner failedRunner)
        {
            _fullSuite = fullSuite;
            _failedRunner = failedRunner;
        }

        public async Task ExecuteAsync()
        {
            var testResults = new List<TestCaseResult>();
            var failedTestCases = new List<FailedTestCase>();

            var retryTests = FailedTestCaseConfiguration.ReadTodayFailedTests();


            if (retryTests.Any())
            {
                testResults.AddRange(
                await _failedRunner.RunAsync(retryTests));
            }
            else
            {
                testResults.AddRange(
                await _fullSuite.RunAsync(failedTestCases));
            }


            FailedTestCaseConfiguration.WriteFailedTests(failedTestCases);
            ReportGenerator.GenerateReport(testResults);
            ConsoleReportPrinter.Print(testResults);
        }
    }
}
