using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Berlinator.Console.Core;

namespace Berlinator.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var config = BerConfig.Load("config.json");

            var app = new BerApp(config);

            AppDomain.CurrentDomain.ProcessExit += app.OnExit;

            await app.Run();

            while (true)
            {
                System.Console.Write("> ");
                var command = System.Console.ReadLine();

                if(string.IsNullOrWhiteSpace(command))
                    continue;

                app.HandleCommand(command);
            }
        }
    }
}
