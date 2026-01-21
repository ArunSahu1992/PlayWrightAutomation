using Quartz;
using Configuration;
using SwatchBharatMission.Automation.Service;
using Execution;
using Serilog;
using Automation.Core;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate:
        "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .WriteTo.File(
        path: "logs/playwright-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        shared: true,
        outputTemplate:
        "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

    IHost host = Host.CreateDefaultBuilder(args)
    .UseSerilog() .
    ConfigureAppConfiguration((hostContext, config) =>
    {
        string sourceRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));

        string folderPath = Path.Combine(sourceRoot, "maharastra");

        if (Directory.Exists(folderPath))
        {
            foreach (var file in Directory.GetFiles(folderPath, "*.json"))
            {
                config.AddJsonFile(file, optional: false, reloadOnChange: true);
            }
        }
    })

    .ConfigureServices((hostContext, services) =>
    {

        string sourceRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));

        string folderPath = Path.Combine(sourceRoot, "maharastra");

        services.Configure<AutomationSettings>(
            hostContext.Configuration.GetSection("AutomationSettings"));

        services.AddSingleton<IConfigurationRegistry>(
      _ => new ConfigurationRegistry(folderPath));

        // Quartz
        services.AddQuartz(q =>
        {
            q.UseMicrosoftDependencyInjectionJobFactory();

            var jobKey = new JobKey("AutomationJob");
            q.AddJob<AutomationJob>(opts => opts.WithIdentity(jobKey));

            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithCronSchedule(
                    hostContext.Configuration["AutomationSettings:Cron"] ?? "*/5 * * * *"));
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        // Automation
        services.AddSingleton<TestExecutor>();
    })
    .Build();

await host.RunAsync();
