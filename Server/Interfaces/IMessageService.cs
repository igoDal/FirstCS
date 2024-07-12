using System.Net.Sockets;

namespace Server.Interfaces;

public interface IMessageService
{
    void SetClientSocket(ISocketWrapper clientSocket);
    void ReadMessage(string loggedInUser);
    void SendMessage();
}