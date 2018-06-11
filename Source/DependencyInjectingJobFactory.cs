using Autofac;
using JobScheduler.JobHelpers;
using Quartz;
using Quartz.Spi;

namespace JobScheduler
{
    class DependencyInjectingJobFactory : IJobFactory
    {
        readonly IContainer _container;

        public DependencyInjectingJobFactory(IContainer container)
        {
            _container = container;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var scope = _container.BeginLifetimeScope();
            var type = bundle.JobDetail.JobType;
            var job = (IJob)_container.Resolve(type);
            return new DependencyInjectedJob(job, scope);
        }

        public void ReturnJob(IJob job)
        {
            var wrappedJob = (DependencyInjectedJob)job;
            wrappedJob.Scope.Dispose();
        }
    }
}
