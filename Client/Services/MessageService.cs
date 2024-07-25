using System;
using System.Text;
using Client.Interfaces;
using Newtonsoft.Json;

namespace Client.Services;

public class MessageService : IMessageService
{
    private ISocketWrapper clientSocket;

    public void SetClientSocket(ISocketWrapper socket)
    {
        clientSocket = socket;
    }

    public void SendMessage(ISocketWrapper socketWrapper)
    {
        SendData("msg");
        
        var request = ReceiveJsonData();
            
        Console.WriteLine($"{request}");
        var username = Console.ReadLine();
        
        SendData(username);
        
        var msg = ReceiveJsonData();
        Console.WriteLine($"{msg}");

        var userMsg = Console.ReadLine();
        SendData(userMsg);
        
        var confirmation = ReceiveJsonData();
        Console.WriteLine($"{confirmation}");
    }

    public void ReadMessage(ISocketWrapper socketWrapper)
    {
        SendData("read");
        
        var msg = ReceiveJsonData();
        Console.WriteLine($"Message: {msg}");
    }
    private void SendData(string data)
    {
        string jsonData = JsonConvert.SerializeObject(new { command = data });
        byte[] messageSent = Encoding.ASCII.GetBytes(jsonData);
        clientSocket.Send(messageSent);
    }
    private string ReceiveJsonData()
    {
        byte[] buffer = new byte[1024];
        int bytesReceived = clientSocket.Receive(buffer);
        string jsonString = Encoding.ASCII.GetString(buffer, 0, bytesReceived);
        dynamic response = JsonConvert.DeserializeObject(jsonString);
        return response;
    }
}