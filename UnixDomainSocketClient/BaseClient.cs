using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnixDomainSocketClient
{
    public abstract class BaseClient
    {
        public int Interval { get; set; } = 1000;
        public bool IsDemon { get; set; } = false;
        public string MsgPrefix { get; set; } = "Test_";
        public int Counter { get; set; } = 0;
        public int BufferSize { get; set; } = 1024;

        protected async Task<string> GetInput()
        {
            if (IsDemon)
            {
                await Task.Delay(Interval);

                Counter++;
                return $"{MsgPrefix}{Counter}";
            }

            Console.WriteLine("[Client]Enter something: ");
            return Console.ReadLine();
        }
    }
}
