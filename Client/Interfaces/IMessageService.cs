namespace Client.Interfaces;

public interface IMessageService
{
    void SetClientSocket(ISocketWrapper socket);
    void SendMessage(ISocketWrapper socketWrapper);
    void ReadMessage(ISocketWrapper socketWrapper);
}