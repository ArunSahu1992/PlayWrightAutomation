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
        private RunTracker _tracker;
        private BaseContext _context;


        public Worker(ILogger<Worker> logger,
            AutomationWorker runner,
            IHostEnvironment hostEnvironment)
        {
            _logger = logger;
            _runner = runner;
            _hostEnvironment = hostEnvironment;
            _tracker =  new RunTracker();
             _context = new BaseContext();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            var flows = new[] { Constants.SWATCHBHARAT_FLOW_NAME, Constants.MANREGA_FLOW_NAME };
            var testResults = new List<TestCaseResult>();
            try
            {
                _context.IsFirstRun =  _tracker.IsFirstRunToday();
                foreach (var flow in flows)
                {
                    try
                    {
                        _context.Flow = flow;
                        _context.AppRootPath = _hostEnvironment.ContentRootPath;
                        testResults.AddRange(await _runner.RunAsync(_context));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message, "Warning for flow : {FlowName}", flow);
                    }
                }
                ReportGenerator.GenerateReport(testResults);
                ConsoleReportPrinter.Print(testResults);
            }
            finally
            {
                _tracker.RecordRun();
                Environment.Exit(0);
            }

        }
    }
}
