using System.Collections.Generic;
using System.Linq;
using Quartz;
using Serilog;

namespace JobScheduler
{
    class QuartzConfigurer
    {
        readonly ILogger _logger;
        readonly List<IMachineSchedule> _machineSchedules;

        public QuartzConfigurer(IEnumerable<IMachineSchedule> machineSchedules, ILogger logger)
        {
            _logger = logger;
            _machineSchedules = machineSchedules.Where(m => m.ShouldApplyToThisMachine).ToList();
        }

        public void Configure(IScheduler scheduler)
        {
            foreach(var machineSchedule in _machineSchedules)
            {
                machineSchedule.ConfigureScheduler(scheduler);
                _logger.Information("Added new schedule: {ScheduleType}", machineSchedule.GetType().Name);
            }

            //Uncomment this line to periodically display the status of the scheduler
            //ScheduleStatusDisplay(scheduler);

            _logger.Information("Finished configuring Quartz.");
        }

        void ScheduleStatusDisplay(IScheduler scheduler)
        {
            var job = JobBuilder.Create<DisplaySchedulerStatusJob>()
                .WithIdentity("Display scheduler status")
                .Build();
            var trigger = TriggerBuilder.Create()
                .WithIdentity("Frequent")
                .WithSimpleSchedule(s => s
                    .WithIntervalInSeconds(10)
                    .RepeatForever()
                    .WithMisfireHandlingInstructionIgnoreMisfires()
                )
                .StartNow()
                .Build();

            scheduler.ScheduleJob(job, trigger);
        }
    }
}
