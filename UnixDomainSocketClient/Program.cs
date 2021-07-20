using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UnixDomainSocketClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var bufferSize = 5;
            var path = @"C:\temp\file.soc";

            var client = new ClientWithAvailable() {
                BufferSize = bufferSize,
                Interval = 0,
                Type = ClientType.Normal,
                ExampleJson = "Examples/SimpleExample.json",
                MsgPrefix = "Test_",
                DisableConsoleOutput = false,
                PrintCounterEveryMilisecond = 0
            };

            Console.WriteLine($"Client type:{client.Type}");
            Console.WriteLine("Press any key to start.");
            Console.ReadKey();
            await client.Start(path);

            Console.ReadLine();
        }

        
    }
}
