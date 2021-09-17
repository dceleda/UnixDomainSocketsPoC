using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var bufferSize = 1024;
            var path = @"C:\temp\nethIpc.soc";
            // var path = @"geth.ipc";

            var client = new ClientWithAvailable() {
                BufferSize = bufferSize,
                Interval = 0,
                Type = ClientType.Normal,
                ExampleJson = "Examples/newPendingTransactions.json",
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
