using Quartz;
using Execution;
using Configuration;
using Microsoft.Extensions.Options;

namespace SwatchBharatMission.Automation.Service
{
    public class AutomationJob
    {
        private readonly TestExecutor _runner;
        public AutomationJob(TestExecutor runner)
        {
            _runner = runner;
        }

        public async Task Execute()
        {
            await _runner.ExecuteAsync();
        }
    }
}
