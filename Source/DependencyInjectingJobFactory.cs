using Autofac;
using JobScheduler.JobHelpers;
using Quartz;
using Quartz.Spi;

namespace JobScheduler
{
    class DependencyInjectingJobFactory : IJobFactory
    {
        readonly ILifetimeScope _lifetimeScope;

        public DependencyInjectingJobFactory(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var scope = _lifetimeScope.BeginLifetimeScope();
            var type = bundle.JobDetail.JobType;
            var job = (IJob)_lifetimeScope.Resolve(type);
            return new DependencyInjectedJob(job, scope);
        }

        public void ReturnJob(IJob job)
        {
            var wrappedJob = (DependencyInjectedJob)job;
            wrappedJob.Scope.Dispose();
        }
    }
}
