using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnixDomainSocketsServer.Common;
using UnixDomainSocketsServer.Model;

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
                _server.Listen(0);

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
                using (clientSocket = _server.EndAccept(ar))
                {
                    _resetEvent.Set();

                    using (NetworkStream stream = new NetworkStream(clientSocket))
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    //using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                    {
                        stream.ReadTimeout = 10000;
                        stream.WriteTimeout = 100000;

                        var jsonReader = new JsonTextReader(reader);
                        jsonReader.SupportMultipleContent = true;

                        while (true)
                        {
                            try
                            {
                                if (!jsonReader.Read())
                                {
                                    break;
                                }

                                JsonSerializer deserializer = new JsonSerializer();
                                Person person = null;

                                try
                                {
                                    person = deserializer.Deserialize<Person>(jsonReader);

                                }
                                catch(IOException exc)
                                {
                                    //jsonReader.
                                }

                                //JsonSerializer serializer = new JsonSerializer();
                                //Console.WriteLine("SERVER: received \"" +  + "\"");
                                if (person != null)
                                {
                                    var serialized = System.Text.Json.JsonSerializer.Serialize<Person>(person);
                                    clientSocket.Send(Encoding.UTF8.GetBytes(serialized.ToUpper()));
                                }
                            }
                            catch (JsonException exc)
                            {
                                clientSocket.Send(Encoding.UTF8.GetBytes($"Error when deserializing: {exc.Message}"));
                                clientSocket.Send(Encoding.UTF8.GetBytes($"Closing connection."));

                                break;
                            }
                        }
                    }
                }
            }
            catch(IOException exc) when (exc.InnerException != null && exc.InnerException is SocketException se && se.SocketErrorCode == SocketError.ConnectionReset)
            {
                Console.WriteLine("Client disconnected.");
                clientSocket?.Dispose();
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
