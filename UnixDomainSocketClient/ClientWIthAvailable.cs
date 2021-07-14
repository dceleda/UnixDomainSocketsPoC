﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UnixDomainSocketClient
{
    public class ClientWIthAvailable : BaseClient
    {
        public async Task Start(string path)
        {
            try
            {
                var endPoint = new UnixDomainSocketEndPoint(path);

                using (var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified))
                {
                    socket.Connect(endPoint);
                    Console.WriteLine($"[Client] Connected to … ..{path}");

                    var bytes = new byte[BufferSize];
                    while (true)
                    {
                        var line = await GetInput();

                        socket.Send(Encoding.UTF8.GetBytes(line));
                        Console.Write("[Client]From Server: ");

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
