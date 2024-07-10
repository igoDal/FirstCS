using System.Net.Sockets;

namespace Server.Interfaces;

public interface IMessageService
{
    void SetClientSocket(Socket clientSocket);
    void ReadMessage(string loggedInUser);
    void SendMessage();
}