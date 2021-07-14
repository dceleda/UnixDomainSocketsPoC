﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnixDomainSocketsServer.Common;

namespace UnixDomainSocketsServer
{
    public class StreamRunner : IDisposable
    {
        private ManualResetEvent _resetEvent = new ManualResetEvent(false);
        private string _path = @"C:\temp\file.soc";
        private Socket _server;

        public void Start()
        {
            var task = Task.Factory.StartNew((x) =>
            {
                StartServer(_path);
            },
            CancellationToken.None,
            TaskCreationOptions.LongRunning);
        }

        private void StartServer(string path)
        {
            try
            {
                DeleteSocketFileIfExists(path);

                var endPoint = new UnixDomainSocketEndPoint(path);

                _server = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

                _server.Bind(endPoint);
                Console.WriteLine($"[Server] Listening … ..{path}");
                _server.Listen(10);

                while (true)
                {
                    _resetEvent.Reset();

                    Console.WriteLine("Waiting for a connection...");
                    _server.BeginAccept(new AsyncCallback(AcceptCallback), null);

                    _resetEvent.WaitOne();
                }
            }
            catch (SocketException exc) when (exc.SocketErrorCode == SocketError.ConnectionReset)
            {

            }
            catch (SocketException exc)
            {
                Console.WriteLine($"{exc.ErrorCode}:{exc.Message}");
            }
            catch (Exception exc)
            {
                Log(exc);
            }
            finally
            {
                Dispose();
            }
        }

        private async void AcceptCallback(IAsyncResult ar)
        {
            Socket clientSocket = null;

            try
            {
                using (Socket client = _server.EndAccept(ar))
                using (NetworkStream stream = new NetworkStream(client))
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    string text;

                    while ((text = await reader.ReadLineAsync()) != null)
                    {
                        Console.WriteLine("SERVER: received \"" + text + "\"");
                        writer.WriteLine(text.ToUpper());
                        writer.Flush();
                    }
                }

                _resetEvent.Set();
            }
            catch (SocketException exc) when (exc.SocketErrorCode == SocketError.ConnectionReset)
            {
                Console.WriteLine("Client disconnected.");
                clientSocket?.Dispose();
            }
            catch (SocketException exc)
            {
                Console.WriteLine($"{exc.ErrorCode}:{exc.Message}");
                clientSocket?.Dispose();
            }
            catch (Exception exc)
            {
                Log(exc);
                clientSocket?.Dispose();
            }
        }

        private void ReadCallback(IAsyncResult ar)
        {
            Socket clientSocket = null;

            try
            {
                StateObject state = ar.AsyncState as StateObject;
                clientSocket = state.ClientSocket;

                // Read data from the client socket.  
                int read = clientSocket.EndReceive(ar);

                // Data was read from the client socket.  
                if (read > 0)
                {
                    var incoming = Encoding.UTF8.GetString(state.Buffer, 0, read);
                    state.MsgBuilder.Append(incoming);

                    if (incoming.Contains("\0"))
                    {
                        // PROCESS the message
                        clientSocket.Send(Encoding.UTF8.GetBytes(state.MsgBuilder.ToString().ToUpper()));
                        state.MsgBuilder.Clear();
                    }

                    clientSocket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                }
                else
                {
                    Console.WriteLine("Closing connection");
                    clientSocket.Dispose();
                }
            }
            catch (SocketException exc) when (exc.SocketErrorCode == SocketError.ConnectionReset)
            {
                Console.WriteLine("IPC client disconnected.");
                clientSocket?.Dispose();
            }
            catch (SocketException exc)
            {
                Console.WriteLine($"{exc.ErrorCode}:{exc.Message}");
                clientSocket?.Dispose();
            }
            catch (Exception exc)
            {
                Log(exc);
                clientSocket?.Dispose();
            }
        }

        private void DeleteSocketFileIfExists(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception exc)
            {
                Log(exc);
            }
        }

        private static void Log(Exception exc)
        {
            Console.WriteLine($"Logger: {exc.ToString()}");
        }

        public void Dispose()
        {
            if (_server != null)
            {
                _server.Dispose();
            }

            DeleteSocketFileIfExists(_path);
        }
    }
}