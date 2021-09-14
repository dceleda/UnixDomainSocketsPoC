using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketClient
{
    public abstract class BaseClient
    {
        public int Interval { get; set; } = 1000;
        public ClientType Type { get; set; } = ClientType.Normal;
        public string ExampleJson { get; set; } = "Examples/SimpleExample.json";
        public string MsgPrefix { get; set; } = "Test_";
        public int Counter { get; set; } = 0;
        public int BufferSize { get; set; } = 1024;
        public bool DisableConsoleOutput { get; set; }
        public int PrintCounterEveryMilisecond { get; set; }

        private Timer _timer;

        public virtual async Task Start(string path)
        {
            if(PrintCounterEveryMilisecond > 0)
            {
                _timer = new Timer(PrintCounter, null, PrintCounterEveryMilisecond, PrintCounterEveryMilisecond);
            }
        }

        private void PrintCounter(object state)
        {
            Console.WriteLine($"Counter:{Counter}");
        }

        protected async Task<string> GetInput()
        {
            switch (Type)
            {
                case ClientType.Demon:
                    await Task.Delay(Interval);

                    Counter++;
                    if (string.IsNullOrEmpty(ExampleJson))
                    {
                        return $"{MsgPrefix}{Counter}";
                    }
                    else
                    {
                        return File.ReadAllText(ExampleJson);
                    }
                default:
                    break;
            }

            Console.WriteLine("[Client]Enter something: ");
            return Console.ReadLine();
        }

        protected void PrintOutput(string text)
        {
            if (!DisableConsoleOutput)
            {
                Console.WriteLine(text);
            }
        }
    }

    public enum ClientType
    {
        Normal = 0,
        Demon = 1
    }
}
