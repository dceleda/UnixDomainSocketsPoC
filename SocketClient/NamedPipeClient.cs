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
        public override async Task Start(string path)
        {
            try
            {
                await base.Start(path);

                var pipeClient =
                    new NamedPipeClientStream(".", path, PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.None);

                pipeClient.Connect();
                
                var ss = new StreamString(pipeClient);

                ss.WriteString("{\"id\": 1, \"method\": \"eth_subscribe\", \"params\": [\"newPendingTransactions\"]}{\"id\": 1, \"method\": \"eth_subscribe\", \"params\": [\"newPendingTransactions\"]}");
                var r = ss.ReadString();
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
            }
            finally
            {
            }
        }
        
        public class StreamString
        {
            private Stream ioStream;
            private UTF8Encoding streamEncoding;

            public StreamString(Stream ioStream)
            {
                this.ioStream = ioStream;
                streamEncoding = new UTF8Encoding();
            }

            public string ReadString()
            {
                var inBuffer = new byte[10000];
                ioStream.Read(inBuffer, 0, 10000);

                return streamEncoding.GetString(inBuffer);
            }

            public int WriteString(string outString)
            {
                byte[] outBuffer = streamEncoding.GetBytes(outString);
                int len = outBuffer.Length;
                if (len > UInt16.MaxValue)
                {
                    len = (int)UInt16.MaxValue;
                }
                // ioStream.WriteByte((byte)(len / 256));
                // ioStream.WriteByte((byte)(len & 255));
                ioStream.Write(outBuffer, 0, len);
                ioStream.Flush();

                return outBuffer.Length + 2;
            }
        }
    }
}