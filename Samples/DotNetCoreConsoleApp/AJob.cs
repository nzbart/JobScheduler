using System;
using System.Threading.Tasks;
using JobScheduler;
using Quartz;
using Serilog;

namespace myapp
{
    public class AJob : LoggingJob
    {
        public AJob(ILogger logger) : base(logger)
        {
        }

        public override Task ExecuteSafe(IJobExecutionContext context)
        {
            Logger.Information("In job.");
            return Task.CompletedTask;
        }
    }
}
