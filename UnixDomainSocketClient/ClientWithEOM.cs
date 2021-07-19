using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UnixDomainSocketClient
{
    public class ClientWithEOM : BaseClient
    {
        public override async Task Start(string path)
        {
            try
            {
                await base.Start(path);

                var endPoint = new UnixDomainSocketEndPoint(path);

                using (var client = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified))
                {
                    client.Connect(endPoint);
                    Console.WriteLine($"[Client] Connected to … ..{path}");

                    var bytes = new byte[BufferSize];
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
                            if (stringReceived.Contains("\0"))
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
    }
}
