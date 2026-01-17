using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Configuration
{
    public class TestCaseResult
    {
        public string TestCaseId { get; set; } = "";
        public string Name { get; set; } = "";
        public bool IsPassed { get; set; }
        public string Status => IsPassed ? "PASSED" : "FAILED";
        public string Message { get; set; } = "";
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double DurationSeconds =>
            (EndTime - StartTime).TotalSeconds;
    }
}
