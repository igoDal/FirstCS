using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class ClientSocket : IClientSocket
    {
        private readonly Socket _socket;

        public ClientSocket(Socket socket)
        {
            _socket = socket;
        }

        public int Send(byte[] buffer) => _socket.Send(buffer);
        public int Receive(byte[] buffer) => _socket.Receive(buffer);
    }
}
