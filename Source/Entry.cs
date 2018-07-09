using System;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Quartz;
using Quartz.Impl;
using Serilog;
using Serilog.Events;

namespace JobScheduler
{
    public class Entry
    {
        const string DefaultLogFilePath = @"Logs/Log.log";

        /// <summary>
        /// Launch the scheduled task runner, which will scan the entry assembly
        /// for instances of IMachineSchedule and IJob.
        /// </summary>
        /// <param name="logFilePath">The log file path template. A date will be appended to the file name in yyyyMMdd format.</param>
        /// <param name="dependencies">Dependencies that your jobs require.</param>
        /// <returns></returns>
        public static async Task RunAsync(string logFilePath = DefaultLogFilePath, IModule dependencies = null) => await RunAsync(null, logFilePath, dependencies);

        /// <summary>
        /// Launch the scheduled task runner, which will scan the entry assembly
        /// for instances of IMachineSchedule and IJob.
        /// </summary>
        /// <param name="stopHandle">A handle that, when signalled, will initiate shutdown of the scheduler.</param>
        /// <param name="logFilePath">The log file path template. A date will be appended to the file name in yyyyMMdd format.</param>
        /// <param name="dependencies">Dependencies that your jobs require.</param>
        /// <returns></returns>
        public static async Task RunAsync(WaitHandle stopHandle, string logFilePath = DefaultLogFilePath, IModule dependencies = null)
        {
            if(logFilePath == null) throw new ArgumentNullException(nameof(logFilePath));

            Log.Logger = BuildLogger(logFilePath);

            try
            {
                using(var container = await ConfigureDependencies(dependencies))
                {
                    var runner = container.Resolve<QuartzRunner>();
                    try
                    {
                        await runner.Start();
                        if(stopHandle != null)
                        {
                            stopHandle.WaitOne();
                        }
                        else
                        {
                            Console.WriteLine("Press 'Q' to quit.");
                            while(char.ToUpperInvariant(Console.ReadKey().KeyChar) != 'Q')
                            {
                                await Task.Delay(100);
                            }

                            Console.WriteLine("Shutting down.");
                        }
                    }
                    catch(Exception ex)
                    {
                        Log.Logger.Error(ex, "There was an error while running Quartz.");
                        throw;
                    }
                    finally
                    {
                        Log.Logger.Information("Stopping Quartz and waiting for jobs to complete...");
                        await runner.Stop();
                        Log.Logger.Information("Quartz stopped.");
                        Log.Logger.Verbose("Dependencies will be disposed.");
                    }
                }
            }
            finally
            {
                Log.Logger.Verbose("Dependencies have been disposed.");
                Log.Logger.Information("Job scheduler has completed.");
            }
        }

        static async Task<IContainer> ConfigureDependencies(IModule dependencies)
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

            if(dependencies != null)
                builder.RegisterModule(dependencies);

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
