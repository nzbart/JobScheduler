using System;
using JobScheduler;
using Quartz;

namespace myapp
{
    class AScheduler : IMachineSchedule
    {
        public bool ShouldApplyToThisMachine => true;

        public void ConfigureScheduler(IScheduler scheduler)
        {
            var job = JobBuilder.Create<AJob>().Build();

            ITrigger trigger = TriggerBuilder.Create()
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(40)
                    .RepeatForever())            
                .Build();

           scheduler.ScheduleJob(job, trigger);
        }
    }
}
