using Quartz;
using Configuration;
using SwatchBharatMission.Automation.Service;
using Execution;
using Serilog;
using Execution.Runner;
using Execution.AutomationFlow;
using Execution.context;
using Execution.Interface;
using TestCases;


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

        // Quartz

        services.Configure<AutomationSettings>(hostContext.Configuration.GetSection("Automation"));

        // Automation
        services.AddSingleton<FullSuiteRunner>();
        services.AddSingleton<TestExecutionService>();
        services.AddSingleton<FailedTestRunner>();
        services.AddHostedService<Worker>();

        services.AddSingleton<IAutomationContextFactory, AutomationContextFactory>();
        services.AddSingleton<IAutomationFlowResolver, AutomationFlowResolver>();

        services.AddSingleton<IAutomationFlow, MnregaAutomationFlow>();
        services.AddSingleton<IAutomationFlow, SwatchBharatAutomationFlow>();

        services.AddSingleton<ITestCase, TC_001_UploadCityDataToServer_MR_05>();
        services.AddSingleton<ITestCase, TC_002_UploadCityDataToServer_MR_3A_SWM>();
        services.AddSingleton<ITestCase, TC_003_UploadCityDataToServer_MR_13A>();
        services.AddSingleton<ITestCase, TC_004_UploadCityDataToServer_MR_3C>();
        services.AddSingleton<ITestCase, TC_005_UploadCityDataToServer_MR_166B>();
        services.AddSingleton<ITestCase, TC_006_UploadCityDataToServer_MN_18_3>();
        services.AddSingleton<ITestCase, TC_007_UploadCityDataToServer_MN_7_1_1>();
        services.AddSingleton<ITestCase, TC_008_UploadCityDataToServer_MN_7_2_2>();
        services.AddSingleton<ITestCase, TC_009_UploadCityDataToServer_MN_7_2_3>();
        services.AddSingleton<ITestCase, TC_010_UploadCityDataToServer_MN_5_6>();
        services.AddSingleton<ITestCase, TC_011_UploadCityDataToServer_MN_6_9>();
        services.AddSingleton<ITestCase, TC_012_UploadCityDataToServer_MN_18_4>();
        services.AddSingleton<ITestCase, TC_013_UploadCityDataToServer_MN_5_11>();
        services.AddSingleton<ITestCase, TC_014_UploadCityDataToServer_MN_6_12>();
        services.AddSingleton<AutomationWorker>();
        services.AddSingleton<AutomationWorker>();

    })
    .Build();

await host.RunAsync();
