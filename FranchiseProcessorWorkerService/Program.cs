using FranchiseProcessorWorkerService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddQuartz(q =>
        {
            var jobKey = new JobKey("FranchiseProcessingJob");
            q.AddJob<FranchiseProcessingJob>(opts => opts.WithIdentity(jobKey));

            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity("FranchiseJobTrigger")
                .WithCronSchedule("0 0/1 * * * ?")); // Run every minute
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
        services.AddSingleton<FranchiseProcessingJob>();
    })
    .Build();

await host.RunAsync();

