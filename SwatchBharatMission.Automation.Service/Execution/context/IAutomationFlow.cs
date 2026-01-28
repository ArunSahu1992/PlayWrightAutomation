using Configuration;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Execution.context
{
    public interface IAutomationFlow
    {
        string Name { get; }
        Task ExecuteAsync(AutomationContext automationContext);
    }
}
