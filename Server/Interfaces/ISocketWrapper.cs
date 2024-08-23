namespace Server.Interfaces;

public interface ISocketWrapper
{
    void Send(byte[] buffer);
    int Receive(byte[] buffer);
    void Close();
}