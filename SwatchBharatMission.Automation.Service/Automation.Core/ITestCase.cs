using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Configuration;
using Microsoft.Extensions.Logging;

namespace Automation.Core
{
    public interface ITestCase
    {
        string TestCaseId { get; }
        Task ExecuteAsync(UploadAutomationSettings uploadAutomationSettings, AutomationSettings automationSettings, ILogger logger);
    }
}
