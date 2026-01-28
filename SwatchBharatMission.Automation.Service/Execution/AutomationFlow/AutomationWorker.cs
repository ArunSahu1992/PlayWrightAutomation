using Execution.context;
using Execution.Interface;
using Microsoft.Extensions.Hosting;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Execution.AutomationFlow
{
    public class AutomationWorker
    {
        private readonly IAutomationContextFactory _contextFactory;
        private readonly IAutomationFlowResolver _flowResolver;

        public AutomationWorker(
            IAutomationContextFactory contextFactory,
            IAutomationFlowResolver flowResolver)
        {
            _contextFactory = contextFactory;
            _flowResolver = flowResolver;
        }

        public async Task RunAsync(string flowName, IHostEnvironment _hostEnvironment)
        {
            var context = _contextFactory.Create(flowName, _hostEnvironment);
            var flow = _flowResolver.Resolve(flowName);

            await flow.ExecuteAsync(context);
        }
    }
}
