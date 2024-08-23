using System.Net.Sockets;
using Server.Interfaces;

namespace Server.Services;

public class SocketWrapper : ISocketWrapper
{
    private readonly Socket _socket;

    public SocketWrapper(Socket socket)
    {
        _socket = socket;
    }

    public void Send(byte[] buffer)
    {
        _socket.Send(buffer);
    }
    public int Receive(byte[] buffer)
    {
        return _socket.Receive(buffer);
    }
    public void Close()
    {
        _socket.Shutdown(SocketShutdown.Both);
        _socket.Close();
    }
}