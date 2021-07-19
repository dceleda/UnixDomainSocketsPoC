using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UnixDomainSocketsServer.Common
{
    public class StateObject
    {
        public StateObject(Socket socket)
        {
            ClientSocket = socket;
        }
        public Socket ClientSocket;
        public const int BufferSize = 1024;
        public byte[] Buffer = new byte[BufferSize];

        public StringBuilder MsgBuilder = new StringBuilder();
    }
}
