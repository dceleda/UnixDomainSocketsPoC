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

            new UnixDomainSocketRunner().StartAsync();
            // wait for server to start
            Thread.Sleep(2000);

            Console.ReadLine();
        }

        private static void StartServer(String path)
        {
            if(File.Exists(path))
            {
                File.Delete(path);
            }

            var endPoint = new UnixDomainSocketEndPoint(path);
            try
            {
                using (var server = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified))
                {
                    server.Bind(endPoint);
                    Console.WriteLine($"[Server] Listening … ..{path}");
                    server.Listen(1);
                    using (Socket accepted = server.Accept())
                    {
                        Console.WriteLine("[Server]Connection Accepted …" + accepted.RemoteEndPoint.ToString());
                        var bytes = new byte[100];
                        while (true)
                        {
                            int byteRecv = accepted.Receive(bytes);
                            String str = Encoding.UTF8.GetString(bytes, 0, byteRecv);
                            Console.WriteLine("[Server]Received " + str);
                            accepted.Send(Encoding.UTF8.GetBytes(str.ToUpper()));
                        }

                    }
                }
            }
            catch(SocketException exc)
            {
                Console.WriteLine($"{exc.ErrorCode}:{exc.Message}");
            }
            catch(Exception exc)
            {
                Console.WriteLine(exc.ToString());
            }
            finally
            {
                try
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
                catch (Exception) { }
            }
        }
    }
}
