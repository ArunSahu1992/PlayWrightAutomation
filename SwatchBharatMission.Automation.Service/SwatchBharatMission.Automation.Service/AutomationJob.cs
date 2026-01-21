using Quartz;
using Execution;
using Configuration;
using Microsoft.Extensions.Options;

namespace SwatchBharatMission.Automation.Service
{
    public class AutomationJob : IJob
    {
        private readonly TestExecutor _runner;
        public AutomationJob(TestExecutor runner)
        {
            _runner = runner;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await _runner.ExecuteAsync();
        }
    }
}
