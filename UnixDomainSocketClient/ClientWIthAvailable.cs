using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UnixDomainSocketClient
{
    public class ClientWithAvailable : BaseClient
    {
        public override async Task Start(string path)
        {
            try
            {
                await base.Start(path);

                var endPoint = new UnixDomainSocketEndPoint(path);

                using (var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified))
                {
                    socket.Connect(endPoint);
                    Console.WriteLine($"[Client] Connected to … ..{path}");

                    var bytes = new byte[BufferSize];
                    while (true)
                    {
                        var line = await GetInput();

                        if (line.ToUpper() == "CLOSE")
                        {
                            socket.Close();
                            break;
                        }
                        else
                        {
                            socket.Send(Encoding.UTF8.GetBytes(line + Environment.NewLine));
                            //socket.Shutdown(SocketShutdown.Send);

                            PrintOutput("[Client]From Server: ");

                            StringBuilder sb = new();
                            int byteRecv = socket.Receive(bytes);
                            while (byteRecv > 0)
                            {
                                var stringReceived = Encoding.UTF8.GetString(bytes, 0, byteRecv);
                                sb.Append(stringReceived);
                                if (byteRecv < BufferSize || byteRecv == BufferSize && socket.Available == 0)
                                {
                                    break;
                                }
                                byteRecv = socket.Receive(bytes);
                            }

                            PrintOutput(sb.ToString());
                        }
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
