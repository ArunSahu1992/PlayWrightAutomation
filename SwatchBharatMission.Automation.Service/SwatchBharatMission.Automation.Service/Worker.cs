using Execution;
using Execution.Runner;

namespace SwatchBharatMission.Automation.Service
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly Orchestrator _runner;


        public Worker(ILogger<Worker> logger,
            Orchestrator runner)
        {
            _logger = logger;
            _runner = runner;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await _runner.ExecuteAsync();
            Environment.Exit(0);
        }
    }
}
