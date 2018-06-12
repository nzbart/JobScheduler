using System;

namespace JobScheduler.JobHelpers
{
    public class CommandLineDetails
    {
        public string FileName { get; }
        public string Arguments { get; }
        public string WorkingDirectory { get; }

        public CommandLineDetails(string fileName, string arguments, string workingDirectory)
        {
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
            WorkingDirectory = workingDirectory ?? throw new ArgumentNullException(nameof(workingDirectory));
        }
    }
}
