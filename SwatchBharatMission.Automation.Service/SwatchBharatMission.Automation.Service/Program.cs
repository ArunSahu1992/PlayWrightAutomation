using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Configuration;
using SwatchBharatMission.Automation.Service;
using Automation.Core;
using Execution;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure<AutomationSettings>(
            hostContext.Configuration.GetSection("AutomationSettings"));

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

        // Automation dependencies
        services.AddSingleton<TestExecutor>();
    })
    .Build();

await host.RunAsync();