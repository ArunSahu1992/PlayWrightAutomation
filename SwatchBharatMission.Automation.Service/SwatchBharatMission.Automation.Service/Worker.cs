using Configuration;
using Execution;
using Execution.AutomationFlow;
using Execution.Runner;

namespace SwatchBharatMission.Automation.Service
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly AutomationWorker _runner;
        private IHostEnvironment _hostEnvironment;


        public Worker(ILogger<Worker> logger,
            AutomationWorker runner,
            IHostEnvironment hostEnvironment)
        {
            _logger = logger;
            _runner = runner;
            _hostEnvironment = hostEnvironment;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            var flows = new[] { Constants.SWATCHBHARAT_FLOW_NAME, Constants.MANREGA_FLOW_NAME };
            try
            {
                foreach (var flow in flows)
                {
                    try
                    {
                        await _runner.RunAsync(flow, _hostEnvironment);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Warning for flow : {FlowName}", flow);
                    }
                }
            }
            finally
            {
                Environment.Exit(0);
            }

        }
    }
}
