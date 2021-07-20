using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnixDomainSocketsServer;

namespace UnixDomainSocketsServer
{
    public class Program
    {
        static void Main(string[] args)
        {

            new StreamRunner().Start();
            // wait for server to start
            Thread.Sleep(2000);

            Console.ReadLine();
        }
    }
}
