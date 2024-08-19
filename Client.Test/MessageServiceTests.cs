using System.Text;
using Client.Interfaces;
using Client.Services;
using Moq;
using Newtonsoft.Json;

namespace Client.Test;

public class MessageServiceTests
{
    [Fact]
    public void SendMessage_ShouldPromptForUsernameAndMessage_AndSendThem()
    {
        // Arrange
        var mockSocket = new Mock<ISocketWrapper>();
        var messageService = new MessageService();
        messageService.SetClientSocket(mockSocket.Object);

        var receiveSequence = new Queue<string>(new[]
        {
            "Enter username:",
            "Enter message:",
            "Message has been sent."
        });

        mockSocket.Setup(socket => socket.Receive(It.IsAny<byte[]>()))
            .Returns((byte[] buffer) =>
            {
                if (receiveSequence.Count == 0)
                    throw new InvalidOperationException("No more data to receive.");

                var message = JsonConvert.SerializeObject(receiveSequence.Dequeue());
                var messageBytes = Encoding.ASCII.GetBytes(message);
                Array.Copy(messageBytes, buffer, messageBytes.Length);
                return messageBytes.Length;
            });

        var inputSequence = new Queue<string>(new[] { "testUser", "Test Message" });
        Console.SetIn(new StringReader(string.Join(Environment.NewLine, inputSequence)));

        // Act
        messageService.SendMessage(mockSocket.Object);

        // Assert
        mockSocket.Verify(socket => socket.Send(It.IsAny<byte[]>()), Times.Exactly(3));
        mockSocket.Verify(socket => socket.Receive(It.IsAny<byte[]>()), Times.Exactly(3));
    }
    
    [Fact]
    public void SendMessage_ShouldDisplayError_WhenUserDoesNotExist()
    {
        // Arrange
        var mockSocket = new Mock<ISocketWrapper>();
        var messageService = new MessageService();
        messageService.SetClientSocket(mockSocket.Object);

        var receiveSequence = new Queue<string>(new[]
        {
            "Enter username:",
            "User doesn't exist."
        });

        mockSocket.Setup(socket => socket.Receive(It.IsAny<byte[]>()))
            .Returns((byte[] buffer) =>
            {
                if (receiveSequence.Count == 0)
                    return 0;

                var message = JsonConvert.SerializeObject(receiveSequence.Dequeue());
                var messageBytes = Encoding.ASCII.GetBytes(message);
                Array.Copy(messageBytes, buffer, messageBytes.Length);
                return messageBytes.Length;
            });

        var inputSequence = new Queue<string>(new[] { "nonexistentUser" });
        Console.SetIn(new StringReader(string.Join(Environment.NewLine, inputSequence)));

        // Act
        messageService.SendMessage(mockSocket.Object);

        // Assert
        mockSocket.Verify(socket => socket.Send(It.IsAny<byte[]>()), Times.Exactly(3));
        mockSocket.Verify(socket => socket.Receive(It.IsAny<byte[]>()), Times.Exactly(3));
    }

    [Fact]
    public void SendMessage_ShouldDisplayError_WhenMessageBoxIsFull()
    {
        // Arrange
        var mockSocket = new Mock<ISocketWrapper>();
        var messageService = new MessageService();
        messageService.SetClientSocket(mockSocket.Object);

        var receiveSequence = new Queue<string>(new[]
        {
            "Enter username:",
            "Mailbox is full."
        });

        mockSocket.Setup(socket => socket.Receive(It.IsAny<byte[]>()))
            .Returns((byte[] buffer) =>
            {
                if (receiveSequence.Count == 0)
                    return 0;

                var message = JsonConvert.SerializeObject(receiveSequence.Dequeue());
                var messageBytes = Encoding.ASCII.GetBytes(message);
                Array.Copy(messageBytes, buffer, messageBytes.Length);
                return messageBytes.Length;
            });

        var inputSequence = new Queue<string>(new[] { "testUser" });
        Console.SetIn(new StringReader(string.Join(Environment.NewLine, inputSequence)));

        // Act
        messageService.SendMessage(mockSocket.Object);

        // Assert
        mockSocket.Verify(socket => socket.Send(It.IsAny<byte[]>()), Times.Exactly(3));
        mockSocket.Verify(socket => socket.Receive(It.IsAny<byte[]>()), Times.Exactly(3));
    }
    
    [Fact]
    public void ReadMessage_ShouldDisplayMessage_WhenMessageIsAvailable()
    {
        // Arrange
        var mockSocket = new Mock<ISocketWrapper>();
        var messageService = new MessageService();
        messageService.SetClientSocket(mockSocket.Object);

        var receiveSequence = new Queue<string>(new[]
        {
            "Test Message"
        });

        mockSocket.Setup(socket => socket.Receive(It.IsAny<byte[]>()))
            .Returns((byte[] buffer) =>
            {
                if (receiveSequence.Count == 0)
                    return 0; 

                var message = JsonConvert.SerializeObject(receiveSequence.Dequeue());
                var messageBytes = Encoding.ASCII.GetBytes(message);
                Array.Copy(messageBytes, buffer, messageBytes.Length);
                return messageBytes.Length;
            });

        // Act
        messageService.ReadMessage(mockSocket.Object);

        // Assert
        mockSocket.Verify(socket => socket.Send(It.IsAny<byte[]>()), Times.Once);
        mockSocket.Verify(socket => socket.Receive(It.IsAny<byte[]>()), Times.Once);
    }
    
    [Fact]
    public void ReadMessage_ShouldDisplayNoNewMessages_WhenNoMessagesExist()
    {
        // Arrange
        var mockSocket = new Mock<ISocketWrapper>();
        var messageService = new MessageService();
        messageService.SetClientSocket(mockSocket.Object);

        var receiveSequence = new Queue<string>(new[]
        {
            "There are no new messages."
        });

        mockSocket.Setup(socket => socket.Receive(It.IsAny<byte[]>()))
            .Returns((byte[] buffer) =>
            {
                if (receiveSequence.Count == 0)
                    return 0;

                var message = JsonConvert.SerializeObject(receiveSequence.Dequeue());
                var messageBytes = Encoding.ASCII.GetBytes(message);
                Array.Copy(messageBytes, buffer, messageBytes.Length);
                return messageBytes.Length;
            });

        // Act
        messageService.ReadMessage(mockSocket.Object);

        // Assert
        mockSocket.Verify(socket => socket.Send(It.IsAny<byte[]>()), Times.Once);
        mockSocket.Verify(socket => socket.Receive(It.IsAny<byte[]>()), Times.Once);
    }
}