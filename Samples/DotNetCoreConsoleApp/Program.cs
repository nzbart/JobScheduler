using System.Threading.Tasks;
using JobScheduler;

namespace myapp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Entry.RunAsync();
        }
    }
}
