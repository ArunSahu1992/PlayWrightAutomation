using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Configuration
{
    /// <summary>
    /// Final outcome of a test execution.
    /// Used to decide retry / persistence.
    /// </summary>
    public class TestCaseResult
    {
        public string TestCaseId { get; set; }
        public bool IsPassed { get; set; }
        public string Message { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status => IsPassed ? "PASSED" : "FAILED";


        public class FailedTestCase
        {
            public string City { get; set; }
            public string TestName { get; set; }
        }
    }
}
