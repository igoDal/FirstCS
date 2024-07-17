using System.Net;

namespace Client.Interfaces;

public interface ISocketWrapper
{
    int Send(byte[] buffer);
    int Receive(byte[] buffer);
    void Connect(EndPoint remoteEndpoint);
    void Close();
}