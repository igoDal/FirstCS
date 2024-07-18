namespace Client.Interfaces;

public interface IMessageService
{
    void SendMessage(string command, ISocketWrapper socketWrapper);
    void ReadMessage(string command, ISocketWrapper socketWrapper);
}