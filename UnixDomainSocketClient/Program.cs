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

            Console.WriteLine("Provide client type (1 - demon or else");
            var clientType = Console.ReadKey();

            if (clientType.KeyChar == '1')
            {
                Console.WriteLine("Provide interval in ms");
                var interval = int.Parse(Console.ReadLine());

                Console.WriteLine("Provide msg prefix");
                var messagePrefix = Console.ReadLine();

                await new ClientWIthAvailable() { BufferSize = bufferSize, Interval = interval, IsDemon = true, MsgPrefix = messagePrefix }.Start(path);
            }
            else
            {
                await new ClientWIthAvailable() { BufferSize = bufferSize }.Start(path);
            }

            Console.ReadLine();
        }

        
    }
}
