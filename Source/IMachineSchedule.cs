using Quartz;

namespace JobScheduler
{
    public interface IMachineSchedule
    {
        bool ShouldApplyToThisMachine { get; }
        void ConfigureScheduler(IScheduler scheduler);
    }
}
