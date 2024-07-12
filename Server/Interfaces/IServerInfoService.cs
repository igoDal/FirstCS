using System.Net.Sockets;

namespace Server.Interfaces;

public interface IServerInfoService
{
    void SetClientSocket(ISocketWrapper clientSocket);
    void InfoCommand();
    void HelpCommand();
    void UptimeCommand();
}