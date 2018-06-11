using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Quartz;
using Quartz.Impl;
using Serilog;
using Serilog.Events;

namespace JobScheduler
{
    public class Entry
    {
        /// <summary>
        /// Launch the scheduled task runner, which will scan the entry assembly
        /// for instances of IMachineSchedule and IJob.
        /// </summary>
        /// <param name="logFilePath">The log file path template. A date will be appended to the file name in yyyyMMdd format.</param>
        /// <returns></returns>
        public static async Task RunAsync(string logFilePath = @"Logs/Log.log")
        {
            Log.Logger = BuildLogger(logFilePath);

            using (var container = await ConfigureDependencies())
            {
                var runner = container.Resolve<QuartzRunner>();
                await runner.Start();
                Console.WriteLine("Press 'Q' to quit.");
                while(char.ToUpperInvariant(Console.ReadKey().KeyChar) != 'Q')
                {
                    await Task.Delay(100);
                }

                Console.WriteLine("Shutting down.");
            }
        }

        static async Task<IContainer> ConfigureDependencies()
        {
            var factory = new StdSchedulerFactory();
            var scheduler = await factory.GetScheduler();

            var builder = new ContainerBuilder();
            builder
                .RegisterAssemblyModules(Assembly.GetEntryAssembly());
            builder
                .RegisterAssemblyTypes(Assembly.GetEntryAssembly())
                .AssignableTo<IMachineSchedule>()
                .As<IMachineSchedule>();
            builder
                .RegisterAssemblyTypes(Assembly.GetEntryAssembly())
                .AssignableTo<IJob>()
                .AsSelf();
            builder
                .RegisterType<QuartzConfigurer>()
                .AsSelf();
            builder.RegisterInstance(scheduler)
                .As<IScheduler>();
            builder
                .RegisterType<QuartzRunner>()
                .AsSelf();
            builder.RegisterType<DisplaySchedulerStatusJob>()
                .AsSelf();
            builder
                .RegisterType<DependencyInjectingJobFactory>()
                .AsSelf();
            builder.RegisterSelf();
            builder.RegisterInstance(Log.Logger)
                .As<ILogger>();
            return builder.Build();
        }

        static ILogger BuildLogger(string logFilePath) => 
            new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithDemystifiedStackTraces()
                .MinimumLevel.Verbose()
                .WriteTo.Console(
                    LogEventLevel.Information,
                    outputTemplate: "[{Timestamp:HH:mm:ss} {JobFireInstanceId} {Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .WriteTo.File(
                    logFilePath,
                    rollingInterval: RollingInterval.Day,
                    encoding: Encoding.UTF8,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {JobFireInstanceId} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();
    }
}
