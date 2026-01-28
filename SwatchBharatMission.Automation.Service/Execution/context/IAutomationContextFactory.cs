using Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Execution.context
{
    public interface IAutomationContextFactory
    {
        AutomationContext Create(string flowName, IHostEnvironment _hostEnvironment);
    }
}
