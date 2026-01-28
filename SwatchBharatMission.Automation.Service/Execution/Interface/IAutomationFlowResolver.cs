using Execution.context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Execution.Interface
{
    public interface IAutomationFlowResolver
    {
        IAutomationFlow Resolve(string flowName);
    }

}
