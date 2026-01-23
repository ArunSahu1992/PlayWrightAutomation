using Quartz;
using Configuration;
using SwatchBharatMission.Automation.Service;
using Execution;
using Serilog;




var logFolder = Path.Combine(AppContext.BaseDirectory, "logs");
Directory.CreateDirectory(logFolder);

var logFile = Path.Combine(logFolder, "playwright-.log");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext().WriteTo.Console(
        outputTemplate:
        "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .WriteTo.File(
        path: logFile,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        shared: true,
        outputTemplate:
        "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

    IHost host = Host.CreateDefaultBuilder(args)
    //.UseSystemd()
    .UseSerilog() .
    ConfigureAppConfiguration((hostContext, config) =>
    {
    })

    .ConfigureServices((hostContext, services) =>
    {

        string sourceRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory));

        string folderPath = Path.Combine(sourceRoot, "maharastra");

        services.Configure<AutomationSettings>(
            hostContext.Configuration.GetSection("AutomationSettings"));

        services.AddSingleton<IConfigurationRegistry>(
      _ => new ConfigurationRegistry(folderPath));

        // Quartz
   


        // Automation
        services.AddSingleton<TestExecutor>();
        services.AddHostedService<Worker>();

    })
    .Build();

await host.RunAsync();
