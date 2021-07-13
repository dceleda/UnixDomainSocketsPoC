using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace UnixDomainSocketClient
{
    class Program
    {
        static void Main(string[] args)
        {
            //var path = Console.ReadLine();
            var path = @"C:\temp\file.soc";
            StartClient(path);
            Console.ReadLine();
        }

        private static void StartClient(String path)
        {

            var endPoint = new UnixDomainSocketEndPoint(path);
            try
            {
                using (var client = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified))
                {
                    client.Connect(endPoint);
                    Console.WriteLine($"[Client] Connected to … ..{path}");
                    String str = String.Empty;
                    var bytes = new byte[100];
                    while (!str.Equals("exit", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Console.WriteLine("[Client]Enter something: ");
                        var line = Console.ReadLine();
                        client.Send(Encoding.UTF8.GetBytes(line));
                        Console.Write("[Client]From Server: ");

                        StringBuilder sb = new();
                        int byteRecv = client.Receive(bytes);                       
                        while(byteRecv > 0)
                        {
                            sb.Append(Encoding.UTF8.GetString(bytes, 0, byteRecv));
                            if(byteRecv < bytes.Length)
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
                try { File.Delete(path); }
                catch { }
            }
        }
    }
}
