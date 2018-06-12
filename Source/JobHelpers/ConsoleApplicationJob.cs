using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Quartz;
using Serilog;

namespace JobScheduler.JobHelpers
{
    public abstract class ConsoleApplicationJob : LoggingJob
    {
        protected ConsoleApplicationJob(ILogger logger) : base(logger)
        {
        }

        public override Task ExecuteSafe(IJobExecutionContext context)
        {
            var commandLine = GetCommandLine();

            var processConfig = new ProcessStartInfo
            {
                FileName = commandLine.FileName,
                Arguments = commandLine.Arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = commandLine.WorkingDirectory
            };

            Logger.Verbose("Executing the following command-line:\r\n{CommandLine}\r\nWorking directory:\r\n{WorkingDirectory}", processConfig.FileName + " " + processConfig.Arguments, processConfig.WorkingDirectory);
            var process = Process.Start(processConfig);
            process.OutputDataReceived += (sender, args) =>
            {
                if (args.Data == null) return;
                Logger.Information("Output: {Output}", args.Data);
            };
            process.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data == null) return;
                Logger.Warning("Error: {Output}", args.Data);
            };
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            if (process.ExitCode != 0)
                throw new InvalidOperationException("Process exited with return code " + process.ExitCode + ".");

            return Task.CompletedTask;
        }

        protected abstract CommandLineDetails GetCommandLine();
    }
}
