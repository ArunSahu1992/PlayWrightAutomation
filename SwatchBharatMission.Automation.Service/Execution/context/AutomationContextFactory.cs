using Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Execution.context
{
    public class AutomationContextFactory : IAutomationContextFactory
    {
        private readonly AutomationSettings _settings;

        public AutomationContextFactory(
            IOptions<AutomationSettings> options)
        {
            _settings = options.Value;
        }


        public AutomationContext Create(string flowName, IHostEnvironment _hostEnvironment)
        {
            if (!_settings.Flows.TryGetValue(flowName, out var flow))
                throw new InvalidOperationException($"Unknown flow: {flowName}");

            if (!flow.Enabled)
                throw new InvalidOperationException(
                    $"Flow '{flowName}' is disabled via configuration");

            return new AutomationContext(flowName, flow, _hostEnvironment);
        }
    }

}
