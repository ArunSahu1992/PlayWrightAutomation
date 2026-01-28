using Execution.context;
using Execution.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Execution.AutomationFlow
{
    public class AutomationFlowResolver : IAutomationFlowResolver
    {
        private readonly IEnumerable<IAutomationFlow> _flows;

        public AutomationFlowResolver(IEnumerable<IAutomationFlow> flows)
        {
            _flows = flows;
        }

        public IAutomationFlow Resolve(string flowName)
        {
            return _flows.First(f =>
                f.Name.Equals(flowName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
