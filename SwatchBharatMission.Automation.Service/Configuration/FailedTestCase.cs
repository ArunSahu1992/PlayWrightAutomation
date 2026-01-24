using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Configuration
{
    public class FailedTestCase
    {
        public string TestName { get; set; }
        public string City { get; set; }
        public string Error { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
