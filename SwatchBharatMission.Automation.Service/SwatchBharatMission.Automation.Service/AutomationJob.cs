using Quartz;
using Execution;
using Configuration;
using Microsoft.Extensions.Options;
using Execution.Runner;

namespace SwatchBharatMission.Automation.Service
{
    public class AutomationJob
    {
        private readonly Orchestrator _runner;
        public AutomationJob(Orchestrator runner)
        {
            _runner = runner;
        }

        public async Task Execute()
        {
            await _runner.ExecuteAsync();
        }
    }
}
