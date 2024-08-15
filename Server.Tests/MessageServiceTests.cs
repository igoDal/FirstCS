﻿using System;
using System.IO;
using System.Text;
using Moq;
using Newtonsoft.Json;
using Xunit;
using Server.Interfaces;
using Server.Services;

namespace Server.Tests
{
    public class MessageServiceTests
    {
        [Fact]
        public void SendMessage_ShouldSendCorrectMessage()
        {
            // Arrange
            var mockSocket = new Mock<ISocketWrapper>();
            var service = new MessageService();
            service.SetClientSocket(mockSocket.Object);

            byte[] capturedMessage = null;

            var receiveSequence = new Queue<string>(new[] { "testUser", "Test Message" });

            mockSocket.Setup(socket => socket.Send(It.IsAny<byte[]>()))
                .Callback<byte[]>(msg => capturedMessage = msg);

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

            File.WriteAllText("testUser.json", JsonConvert.SerializeObject(new { Userame = "testUser", Password = "password", Role = "user" }));
            File.WriteAllText("testUser_msg.txt", string.Empty);

            // Act
            service.SendMessage();

            // Assert
            string expectedMessage = "Message has been sent.";
            string jsonExpectedMessage = JsonConvert.SerializeObject(expectedMessage);
            byte[] expectedMessageBytes = Encoding.ASCII.GetBytes(jsonExpectedMessage);

            Assert.NotNull(capturedMessage);
            Assert.Equal(Encoding.ASCII.GetString(expectedMessageBytes), Encoding.ASCII.GetString(capturedMessage));

            File.Delete("testUser.json");
            File.Delete("testUser_msg.txt");
        }
        
        [Fact]
        public void SendMessage_ShouldReturnUserDoesNotExist_WhenUserFileDoesNotExist()
        {
            // Arrange
            var mockSocket = new Mock<ISocketWrapper>();
            var service = new MessageService();
            service.SetClientSocket(mockSocket.Object);

            byte[] capturedMessage = null;

            var receiveSequence = new Queue<string>(new[] { "nonexistentUser", "Test Message" });

            mockSocket.Setup(socket => socket.Send(It.IsAny<byte[]>()))
                .Callback<byte[]>(msg => capturedMessage = msg);

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

            // Act
            service.SendMessage();

            // Assert
            string expectedMessage = "User doesn't exist.";
            string jsonExpectedMessage = JsonConvert.SerializeObject(expectedMessage);
            byte[] expectedMessageBytes = Encoding.ASCII.GetBytes(jsonExpectedMessage);

            Assert.NotNull(capturedMessage);
            Assert.Equal(Encoding.ASCII.GetString(expectedMessageBytes), Encoding.ASCII.GetString(capturedMessage));
        }
        
        [Fact]
        public void SendMessage_ShouldReturnMailboxFull_WhenMessageCountExceedsLimit()
        {
            // Arrange
            var mockSocket = new Mock<ISocketWrapper>();
            var service = new MessageService();
            service.SetClientSocket(mockSocket.Object);

            byte[] capturedMessage = null;

            //Note: username should be different than in other tests, as it could fail the test ("User doesn't exist.")
            //due to shared state.
            var receiveSequence = new Queue<string>(new[] { "testUser123", "Test Message" });

            mockSocket.Setup(socket => socket.Send(It.IsAny<byte[]>()))
                .Callback<byte[]>(msg => capturedMessage = msg);

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

            File.WriteAllText("testUser123.json", JsonConvert.SerializeObject(new { Userame = "testUser123", Password = "password", Role = "user" }));
            File.WriteAllLines("testUser123_msg.txt", new[] { "msg1", "msg2", "msg3", "msg4", "msg5" });

            // Act
            service.SendMessage();

            // Assert
            string expectedMessage = "Mailbox is full.";
            string jsonExpectedMessage = JsonConvert.SerializeObject(expectedMessage);
            byte[] expectedMessageBytes = Encoding.ASCII.GetBytes(jsonExpectedMessage);

            Assert.NotNull(capturedMessage);
            Assert.Equal(Encoding.ASCII.GetString(expectedMessageBytes), Encoding.ASCII.GetString(capturedMessage));

            File.Delete("testUser123.json");
            File.Delete("testUser123_msg.txt");
        }

    }
}
