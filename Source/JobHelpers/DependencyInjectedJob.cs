using System;
using System.Threading.Tasks;
using Quartz;

namespace JobScheduler.JobHelpers
{
    class DependencyInjectedJob : IJob
    {
        readonly IJob _innerJob;
        public IDisposable Scope { get; }

        public DependencyInjectedJob(IJob innerJob, IDisposable scope)
        {
            _innerJob = innerJob;
            Scope = scope;
        }

        public Task Execute(IJobExecutionContext context)
        {
            return _innerJob.Execute(context);
        }
    }
}
