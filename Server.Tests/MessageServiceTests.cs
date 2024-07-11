using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Server.Services;
using Xunit;

namespace Server.Tests
{
    public class MessageServiceTests : IDisposable
    {
        private readonly Socket _serverSocket;
        private readonly Socket _clientSocket;
        private readonly MessageService _messageService;
        private readonly NetworkStream _clientStream;
        private readonly StreamWriter _clientWriter;
        private readonly StreamReader _clientReader;

        public MessageServiceTests()
        {
            // Create a pair of connected sockets
            (_serverSocket, _clientSocket) = CreateSocketPair();

            _messageService = new MessageService();
            _messageService.SetClientSocket(_serverSocket);

            _clientStream = new NetworkStream(_clientSocket);
            _clientWriter = new StreamWriter(_clientStream, Encoding.ASCII) { AutoFlush = true };
            _clientReader = new StreamReader(_clientStream, Encoding.ASCII);
        }

        [Fact]
        public void SendMessage_ShouldSendCorrectResponses()
        {
            // Arrange
            var username = "testuser";
            var message = "Hello, this is a test message.";
            var userFile = $"{username}.json";
            var msgFile = $"{username}_msg.txt";
            var user = new User() { Userame = username, Password = "password", Role = "user" };

            // Ensure user file exists
            File.WriteAllText(userFile, JsonConvert.SerializeObject(user));

            // Act in a separate thread to avoid blocking
            var serverTask = Task.Run(() => _messageService.SendMessage());

            // Simulate client behavior
            Assert.Equal("Enter username:", ReadMessageFromServer());
            SendMessageToServer(username);

            Assert.Equal("Type your message:", ReadMessageFromServer());
            SendMessageToServer(message);

            Assert.Equal("Message has been sent.", ReadMessageFromServer());

            // Wait for the server task to complete
            serverTask.Wait();

            // Verify that the message was saved in the correct file
            var messages = File.ReadAllLines(msgFile);
            Assert.Contains(message, messages);

            // Cleanup
            File.Delete(userFile);
            File.Delete(msgFile);
        }

        private (Socket serverSocket, Socket clientSocket) CreateSocketPair()
        {
            var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            listener.Listen(1);

            var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Connect(listener.LocalEndPoint);
            var server = listener.Accept();

            listener.Close();

            return (server, client);
        }

        private void SendMessageToServer(string message)
        {
            var jsonMsg = JsonConvert.SerializeObject(message);
            _clientWriter.WriteLine(jsonMsg);
        }

        private string ReadMessageFromServer()
        {
            var jsonMsg = _clientReader.ReadLine();
            return JsonConvert.DeserializeObject<string>(jsonMsg);
        }

        public void Dispose()
        {
            _clientWriter.Dispose();
            _clientReader.Dispose();
            _clientStream.Dispose();
            _serverSocket.Dispose();
            _clientSocket.Dispose();
        }
    }
}
