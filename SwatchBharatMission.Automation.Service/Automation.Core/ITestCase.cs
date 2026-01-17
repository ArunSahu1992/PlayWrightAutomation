using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Configuration;

namespace Automation.Core
{
    public interface ITestCase
    {
        string TestCaseId { get; }
        Task ExecuteAsync(TestContext context,  Dictionary<string, string> parameters, IOptions<AutomationSettings> options);
    }
}
