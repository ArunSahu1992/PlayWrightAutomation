using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
using Microsoft.Extensions.Logging;

namespace Configuration
{
    public interface ITestCase
    {
        string TestCaseId { get; }
        Task ExecuteAsync(AutomationContext automationContext, ILogger logger);
    }
}
