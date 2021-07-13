using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UnixDomainSocketClient
{
    class Program
    {
        private static bool _isDemon = false;
        private static int _interval = 1000;
        private static string _messagePrefix = "Test_";
        private static int _counter = 0;

        static async Task Main(string[] args)
        {
            var path = @"C:\temp\file.soc";

            Console.WriteLine("Provide client type (1 - demon or else");
            var clientType = Console.ReadKey();

            if(clientType.KeyChar == '1')
            {
                _isDemon = true;

                Console.WriteLine("Provide interval in ms");
                _interval = int.Parse(Console.ReadLine());

                Console.WriteLine("Provide msg prefix");
                _messagePrefix = Console.ReadLine();

                await StartClientAsync(path);
            }
            

            Console.ReadLine();
        }

        private static async Task StartClientAsync(String path)
        {
            try
            {
                var endPoint = new UnixDomainSocketEndPoint(path);

                using (var client = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified))
                {
                    client.Connect(endPoint);
                    Console.WriteLine($"[Client] Connected to … ..{path}");

                    var bytes = new byte[5];
                    while (true)
                    {                       
                        var line = await GetInput();

                        client.Send(Encoding.UTF8.GetBytes(line + "\0"));
                        Console.Write("[Client]From Server: ");

                        StringBuilder sb = new();
                        int byteRecv = client.Receive(bytes);                       
                        while (byteRecv > 0)
                        {
                            var stringReceived = Encoding.UTF8.GetString(bytes, 0, byteRecv);
                            sb.Append(stringReceived);
                            if(stringReceived.Contains("\0"))
                            {
                                break;
                            }
                            byteRecv = client.Receive(bytes);
                        }

                        Console.WriteLine(sb.ToString());
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
            }
            finally
            {
            }
        }

        private static async Task<string> GetInput()
        {
            if (_isDemon)
            {
                await Task.Delay(_interval);

                _counter++;
                return $"{_messagePrefix}{_counter}";
            }

            Console.WriteLine("[Client]Enter something: ");
            return Console.ReadLine();
        }
    }
}
