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


        public AutomationContext Create(BaseContext  _baseContext )
        {
            if (!_settings.Flows.TryGetValue(_baseContext.Flow, out var flow))
                throw new InvalidOperationException($"Unknown flow: {_baseContext.Flow}");

            if (!flow.Enabled)
                throw new InvalidOperationException(
                    $"Flow '{_baseContext.Flow}' is disabled via configuration");

            return new AutomationContext(flow, _baseContext);
        }
    }

}
