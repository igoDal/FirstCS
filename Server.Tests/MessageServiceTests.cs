using Server;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Xunit;
using Moq;
using Newtonsoft.Json;

public class MessageServiceTests
{
    private readonly MessageService _messageService;
    private readonly Mock<Socket> _mockSocket;

    public MessageServiceTests()
    {
        _mockSocket = new Mock<Socket>(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _messageService = new MessageService();
        _messageService.SetClientSocket(_mockSocket.Object);
    }

    [Fact]
    public void SendMessage_ShouldSendMessageToExistingUser()
    {
        // Arrange
        var username = "testuser";
        var message = "Hello, this is a test message!";
        var expectedFilePath = $"{username}.json";
        var expectedMsgFilePath = $"{username}_msg.txt";

        CreateTestUser(username);

        _mockSocket.SetupSequence(s => s.Receive(It.IsAny<byte[]>()))
            .Returns(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(username)).Length)
            .Returns(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(message)).Length);

        // Act
        _messageService.SendMessage();

        // Assert
        Assert.True(File.Exists(expectedMsgFilePath));
        var messages = File.ReadAllLines(expectedMsgFilePath);
        Assert.Contains(message, messages);
        
        DeleteTestFiles(username);
    }

    private void CreateTestUser(string username)
    {
        var user = new User()
        {
            Userame = username,
            Password = "password123",
            Role = "user"
        };

        using (StreamWriter file = File.CreateText($"{username}.json"))
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(file, user);
        }
    }

    private void FillMailbox(string username)
    {
        var msgFile = $"{username}_msg.txt";
        for (int i = 0; i < 5; i++)
        {
            File.AppendAllText(msgFile, $"Test message {i + 1}\n");
        }
    }

    private void DeleteTestFiles(string username)
    {
        var userFile = $"{username}.json";
        var msgFile = $"{username}_msg.txt";

        if (File.Exists(userFile))
        {
            File.Delete(userFile);
        }

        if (File.Exists(msgFile))
        {
            File.Delete(msgFile);
        }
    }
}
