using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnixDomainSocketsServer
{
    public class UnixDomainSocketRunner : IDisposable
    {
        private ManualResetEvent _resetEvent = new ManualResetEvent(false);
        private string _path = @"C:\temp\file.soc";
        private Socket _server;

        public void StartAsync()
        {
            var task = Task.Factory.StartNew((x) =>
            {
                StartAsyncServer(_path);
            }, 
            CancellationToken.None, 
            TaskCreationOptions.LongRunning);
        }

        private void StartAsyncServer(string path)
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

        private void AcceptCallback(IAsyncResult ar)
        {
            Socket clientSocket = null;

            try
            {
                clientSocket = _server.EndAccept(ar);

                _resetEvent.Set();

                StateObject state = new(clientSocket);
                clientSocket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, SocketFlags.None, new AsyncCallback(ReadCallback), state);
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
            if(_server != null)
            {
                _server.Dispose();
            }

            DeleteSocketFileIfExists(_path);
        }
    }

    public class StateObject
    {
        public StateObject(Socket socket)
        {
            ClientSocket = socket;
        }
        public Socket ClientSocket;
        public const int BufferSize = 5;
        public byte[] Buffer = new byte[BufferSize];

        public StringBuilder MsgBuilder = new StringBuilder();
    }

    static class SocketExtensions
    {
        public static bool IsConnected(this Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException) { return false; }
        }
    }
}
