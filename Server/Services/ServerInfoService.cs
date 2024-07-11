using System;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Server.Interfaces;

namespace Server.Services;

public class ServerInfoService : IServerInfoService
{
    private readonly string serverVersion;
    private readonly DateTime serverCreationDate;
    private Socket clientSocket;

    public ServerInfoService(string version, DateTime creationDate)
    {
        serverVersion = version;
        serverCreationDate = creationDate;
    }

    public void SetClientSocket(Socket socket)
    {
        clientSocket = socket;
    }

    public void InfoCommand()
    {
        var message = $"Server version: {serverVersion}\nServer Creation Date: {serverCreationDate}";
        SendData(message);
    }

    public void HelpCommand()
    {
        var message = $"Available commands:\n" +
                      $"'add' - to add new user\n" +
                      $"'help' - to get a list of available commands with their description\n" +
                      $"'info' - to get info about server version, server creation date\n" +
                      $"'msg' - to send a message to other user\n" +
                      $"'read' - to read next message\n" +
                      $"'uptime' - to check server uptime\n" +
                      $"'user' - to print user data\n" +
                      $"'stop' - to stop the server\n" +
                      $"'logout' - to log out";
        SendData(message);
    }

    public void UptimeCommand()
    {
        var serverCurrentDate = DateTime.Now;
        var message = $"Server is up for {serverCurrentDate - serverCreationDate}";
        SendData(message);
    }

    private void SendData(string message)
    {
        var jsonMsg = JsonConvert.SerializeObject(message);
        var msg = Encoding.ASCII.GetBytes(jsonMsg);
        clientSocket.Send(msg);
    }
}