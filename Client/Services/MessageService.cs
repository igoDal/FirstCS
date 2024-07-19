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
        string jsonCommand = JsonConvert.SerializeObject("Enter your message: ");
        byte[] msgCommand = Encoding.ASCII.GetBytes(jsonCommand);
        socketWrapper.Send(msgCommand);

        byte[] msgReceived = new byte[1024];
        int byteRcvd = socketWrapper.Receive(msgReceived);
        string jsonString = Encoding.ASCII.GetString(msgReceived, 0, byteRcvd);
        string encodingString = JsonConvert.DeserializeObject(jsonString).ToString();
            
        // Handle the message sending logic here, e.g., save it to a file or database
            
        Console.WriteLine($"Message received: {encodingString}");
    }

    public void ReadMessage(ISocketWrapper socketWrapper)
    {
        string jsonCommand = JsonConvert.SerializeObject("Reading messages: ");
        byte[] msgCommand = Encoding.ASCII.GetBytes(jsonCommand);
        socketWrapper.Send(msgCommand);

        // Handle the message reading logic here, e.g., read from a file or database
        string messages = "Your messages go here"; // Placeholder for actual messages

        string jsonResponse = JsonConvert.SerializeObject(messages);
        byte[] msgResponse = Encoding.ASCII.GetBytes(jsonResponse);
        socketWrapper.Send(msgResponse);
    }
}