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

            Console.WriteLine(@"Provide client type (1 - demon, 2 - JSON, other)");
            var clientType = Console.ReadKey();

            switch (clientType.KeyChar)
            {
                case '1':
                    {
                        Console.WriteLine("Provide interval in ms");
                        var interval = int.Parse(Console.ReadLine());

                        Console.WriteLine("Provide msg prefix");
                        var messagePrefix = Console.ReadLine();

                        await new ClientWithAvailable() { BufferSize = bufferSize, Interval = interval, Type = ClientType.Demon, MsgPrefix = messagePrefix }.Start(path);
                        break;
                    }
                case '2':
                    {
                        await new ClientWithAvailable() { BufferSize = bufferSize, Type = ClientType.JsonSender01, ExampleJson = "Examples/SimpleIncorrect.json" }.Start(path);
                        break;
                    }
                default:
                    await new ClientWithAvailable() { BufferSize = bufferSize }.Start(path);
                    break;
            }

            Console.ReadLine();
        }

        
    }
}
