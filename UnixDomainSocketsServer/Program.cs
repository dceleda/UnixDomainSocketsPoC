using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnixDomainSocketsPoC
{
    class Program
    {
        private static ManualResetEvent _resetEvent = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            var path = @"C:\temp\file.soc";
            //string path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            var task = Task.Factory.StartNew((x) =>
            {
                StartAsyncServer(path);
            }, CancellationToken.None, TaskCreationOptions.LongRunning );

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
                    File.Delete(path); 
                }
                catch { }
            }
        }

        private static void StartAsyncServer(string path)
        {
            if (File.Exists(path))
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
                    server.Listen(10);

                    while(true)
                    {
                        _resetEvent.Reset();

                        Console.WriteLine("Waiting for a connection...");
                        server.BeginAccept(new AsyncCallback(AcceptCallback), server);

                        _resetEvent.WaitOne();
                    }
                }
            }
            catch (SocketException exc)
            {
                Console.WriteLine($"{exc.ErrorCode}:{exc.Message}");
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
            }
            finally
            {
                try
                {
                    File.Delete(path);
                }
                catch { }
            }
        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            var server = ar.AsyncState as Socket;
            var clientSocket = server?.EndAccept(ar);

            _resetEvent.Set();

            StateObject state = new(clientSocket);
            clientSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None, new AsyncCallback(ReadCallback), state);
        }

        private static void ReadCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket clientSocket = state.ClientSocket;

            // Read data from the client socket.  
            int read = clientSocket.EndReceive(ar);

            // Data was read from the client socket.  
            if (read > 0)
            {
               var incoming = Encoding.UTF8.GetString(state.buffer, 0, read);

                clientSocket.Send(Encoding.UTF8.GetBytes(incoming.ToUpper()));

                clientSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
            }
            else
            {
                Console.WriteLine("Close=ing connection");
                clientSocket.Close();
            }
        }
    }

    public class StateObject
    {
        public StateObject(Socket socket)
        {
            ClientSocket = socket;
        }
        public Socket ClientSocket;
        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];
    }
}
