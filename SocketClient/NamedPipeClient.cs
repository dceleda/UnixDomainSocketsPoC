using System;
using System.IO;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SocketClient
{
    public class NamedPipeClient : BaseClient
    {
        private UTF8Encoding _streamEncoding = new ();
        
        public override async Task Start(string path)
        {
            try
            {
                await base.Start(path);

                var pipeClient =
                    new NamedPipeClientStream(".", path, PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.None);

                pipeClient.Connect();

                // Task.Factory.StartNew(() => );
                
                while (true)
                {
                    var line = await GetInput();
                    WriteString(line, pipeClient);
                    ReadString(pipeClient);
                }

                // ss.WriteString("{\"id\": 1, \"method\": \"eth_subscribe\", \"params\": [\"newPendingTransactions\"]}{\"id\": 1, \"method\": \"eth_subscribe\", \"params\": [\"newPendingTransactions\"]}");
                
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
            }
            finally
            {
            }
            
            void ReadString(NamedPipeClientStream client)
            {
                var inBuffer = new byte[BufferSize];
                StringBuilder sb = new();
                int byteRecv = client.Read(inBuffer);
                
                while (byteRecv > 0)
                {
                    var stringReceived = Encoding.UTF8.GetString(inBuffer, 0, byteRecv);
                    sb.Append(stringReceived);
                    if (byteRecv < BufferSize)
                    {
                        PrintOutput(sb.ToString());
                        sb.Clear();

                        return;
                    }

                    byteRecv = client.Read(inBuffer);
                }
            }

            int WriteString(string outString, NamedPipeClientStream client)
            {
                byte[] outBuffer = _streamEncoding.GetBytes(outString);
                int len = outBuffer.Length;
                if (len > UInt16.MaxValue)
                {
                    len = (int)UInt16.MaxValue;
                }
                // ioStream.WriteByte((byte)(len / 256));
                // ioStream.WriteByte((byte)(len & 255));
                client.Write(outBuffer, 0, len);
                client.Flush();

                return outBuffer.Length + 2;
            }
        }
    }
}