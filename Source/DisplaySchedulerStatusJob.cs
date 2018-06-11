using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl.Matchers;
using Serilog;

namespace JobScheduler
{
    class DisplaySchedulerStatusJob :  IJob
    {
        readonly IScheduler _scheduler;
        readonly ILogger _logger;

        public DisplaySchedulerStatusJob(IScheduler scheduler, ILogger logger)
        {
            _scheduler = scheduler;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var status = await GetSchedulerStatus(_scheduler);
            _logger.Verbose("Current scheduler status: {Status}", status);
        }

        static async Task<string> GetSchedulerStatus(IScheduler scheduler)
        {
            StringBuilder output = new StringBuilder();
            var jobGroups = await scheduler.GetJobGroupNames();

            foreach (var group in jobGroups)
            {
                var groupMatcher = GroupMatcher<JobKey>.GroupContains(group);
                var jobKeys = await scheduler.GetJobKeys(groupMatcher);
                foreach (var jobKey in jobKeys)
                {
                    var detail = await scheduler.GetJobDetail(jobKey);
                    var triggers = await scheduler.GetTriggersOfJob(jobKey);
                    foreach (ITrigger trigger in triggers)
                    {
                        output.AppendLine(group);
                        output.AppendLine(jobKey.Name);
                        output.AppendLine(detail.Description);
                        output.AppendLine(trigger.Key.Name);
                        output.AppendLine(trigger.Key.Group);
                        output.AppendLine(trigger.GetType().Name);
                        output.AppendLine((await scheduler.GetTriggerState(trigger.Key)).ToString());
                        DateTimeOffset? nextFireTime = trigger.GetNextFireTimeUtc();
                        if (nextFireTime.HasValue)
                        {
                            output.AppendLine(nextFireTime.Value.LocalDateTime.ToString(CultureInfo.InvariantCulture));
                        }

                        DateTimeOffset? previousFireTime = trigger.GetPreviousFireTimeUtc();
                        if (previousFireTime.HasValue)
                        {
                            output.AppendLine(previousFireTime.Value.LocalDateTime.ToString(CultureInfo.InvariantCulture));
                        }
                    }
                }
            }

            return output.ToString();
        }
    }
}
