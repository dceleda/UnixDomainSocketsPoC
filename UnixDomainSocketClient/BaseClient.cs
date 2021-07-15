using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnixDomainSocketClient
{
    public abstract class BaseClient
    {
        public int Interval { get; set; } = 1000;
        public ClientType Type { get; set; } = ClientType.Normal;
        public string ExampleJson { get; set; } = "Examples/SimpleExample.json";
        public string MsgPrefix { get; set; } = "Test_";
        public int Counter { get; set; } = 0;
        public int BufferSize { get; set; } = 1024;

        protected async Task<string> GetInput()
        {
            switch (Type)
            {
                case ClientType.Demon:
                    await Task.Delay(Interval);

                    Counter++;
                    return $"{MsgPrefix}{Counter}";
                case ClientType.JsonSender01:
                    Console.WriteLine("Press any key to send JSON");
                    Console.ReadKey();
                    Console.WriteLine();
                    return File.ReadAllText(ExampleJson);
                default:
                    break;
            }

            Console.WriteLine("[Client]Enter something: ");
            return Console.ReadLine();
        }
    }

    public enum ClientType
    {
        Normal = 0,
        Demon = 1,
        JsonSender01 = 2
    }
}
