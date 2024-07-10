namespace Server.Interfaces;

public interface IServerSocket
{
    void SendData(string message);
    string ReceiveData();
}