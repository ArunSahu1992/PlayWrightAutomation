using Quartz;
using Execution;
using Configuration;
using Microsoft.Extensions.Options;

namespace SwatchBharatMission.Automation.Service
{
    public class AutomationJob : IJob
    {
        private readonly TestExecutor _runner;
        private readonly IOptions<AutomationSettings> _options;
        public AutomationJob(TestExecutor runner, IOptions<AutomationSettings> options)
        {
            _runner = runner;
            _options = options;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await _runner.ExecuteAsync();
        }
    }
}
