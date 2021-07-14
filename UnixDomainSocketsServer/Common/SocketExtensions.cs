using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UnixDomainSocketsServer.Common
{
    static class SocketExtensions
    {
        public const int FIONREAD = 0x4004667F;

        public static (uint BytesPending, int BytesAvailable) GetPendingByteCount(this Socket s)
        {
            byte[] outValue = BitConverter.GetBytes(0);

            // Check how many bytes have been received.
            s.IOControl(FIONREAD, null, outValue);

            uint bytesAvailable = BitConverter.ToUInt32(outValue, 0);

            return (bytesAvailable, s.Available);
        }

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
