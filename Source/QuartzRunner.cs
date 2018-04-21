using System.Threading.Tasks;
using Quartz;
using Serilog;

namespace JobScheduler
{
    sealed class QuartzRunner
    {
        readonly QuartzConfigurer _configurer;
        readonly ILogger _logger;
        readonly DependencyInjectingJobFactory _dependencyInjectingJobFactory;
        readonly IScheduler _scheduler;

        public QuartzRunner(QuartzConfigurer configurer, ILogger logger, IScheduler scheduler, DependencyInjectingJobFactory dependencyInjectingJobFactory)
        {
            _configurer = configurer;
            _logger = logger;
            _scheduler = scheduler;
            _dependencyInjectingJobFactory = dependencyInjectingJobFactory;
        }

        public async Task Start()
        {
            _scheduler.JobFactory = _dependencyInjectingJobFactory;
            _configurer.Configure(_scheduler);
            await _scheduler.Start();
            _logger.Information("Quartz started.");
        }
    }
}
