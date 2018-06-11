namespace JobScheduler.JobHelpers
{
    public class CommandLineDetails
    {
        public string FileName { get; }
        public string Arguments { get; }

        public CommandLineDetails(string fileName, string arguments)
        {
            FileName = fileName;
            Arguments = arguments;
        }
    }
}
