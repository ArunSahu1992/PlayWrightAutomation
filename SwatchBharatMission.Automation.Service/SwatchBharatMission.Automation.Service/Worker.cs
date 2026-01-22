using Execution;

namespace SwatchBharatMission.Automation.Service
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly TestExecutor _runner;


        public Worker(ILogger<Worker> logger,
            TestExecutor runner)
        {
            _logger = logger;
            _runner = runner;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            AutomationJob ob = new AutomationJob(_runner);
            await ob.Execute();
        }
    }
}
