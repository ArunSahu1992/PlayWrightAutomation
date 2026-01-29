using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Configuration
{
    public class FailedTestCase
    {
        public string City { get; set; } = string.Empty;
        public string TenantCode { get; set; } = string.Empty;
        public string TestName { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = new DateTime().Date;
    }
}
