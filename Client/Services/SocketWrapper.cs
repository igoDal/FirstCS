using System.Net;
using System.Net.Sockets;
using Client.Interfaces;

namespace Client.Services;

public class SocketWrapper : ISocketWrapper
{
    private readonly Socket _socket;

    public SocketWrapper(Socket socket)
    {
        _socket = socket;
    }

    public int Send(byte[] buffer)
    {
        return _socket.Send(buffer);
    }

    public int Receive(byte[] buffer)
    {
        return _socket.Receive(buffer);
    }
    public void Connect(EndPoint remoteEndpoint)
    {
        _socket.Connect(remoteEndpoint);
    }
    public void Close()
    {
        _socket.Close();
    }
}