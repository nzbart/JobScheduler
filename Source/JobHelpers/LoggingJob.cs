using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Quartz;
using Serilog;
using Serilog.Context;

namespace JobScheduler.JobHelpers
{
    public abstract class LoggingJob : IJob
    {
        protected ILogger Logger { get; }

        protected LoggingJob(ILogger logger)
        {
            Logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using(LogContext.PushProperty("JobFireInstanceId", context.FireInstanceId))
            {
                try
                {
                    var jobType = GetType().Name;
                    Logger.Information("Starting job {JobType}.", jobType);
                    var timer = Stopwatch.StartNew();

                    await ExecuteSafe(context);

                    timer.Stop();
                    Logger.Information("Job {JobType} completed in {TimeTaken}.", jobType, timer.Elapsed);
                }
                catch(Exception ex)
                {
                    throw new JobExecutionException(ex, false);
                }
            }
        }

        public abstract Task ExecuteSafe(IJobExecutionContext context);
    }
}
