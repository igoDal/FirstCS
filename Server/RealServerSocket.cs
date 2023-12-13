using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class RealServerSocket : IRealServerSocket
    {
        private readonly Socket _socket;

        public RealServerSocket(Socket socket)
        {
            _socket = socket;
        }
        public int Receive(byte[] buffer) => _socket.Receive(buffer);

        public int Send(byte[] buffer) => _socket.Send(buffer);
    }
}
