using System.Text;
using Client.Interfaces;
using Client.Services;
using Moq;
using Newtonsoft.Json;

namespace Client.Test;

public class UserServiceTests
    {
        private readonly Mock<ISocketWrapper> _mockSocketWrapper;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockSocketWrapper = new Mock<ISocketWrapper>();
            _userService = new UserService(_mockSocketWrapper.Object);
        }

        [Fact]
        public void Login_ShouldReturnTrue_WhenCredentialsAreCorrect()
        {
            // Arrange
            var username = "testuser";
            var password = "password123";
            var loginPrompt = JsonConvert.SerializeObject(new { command = "Enter username:" });
            var passwordPrompt = JsonConvert.SerializeObject(new { command = "Enter password:" });
            var loginResponse = JsonConvert.SerializeObject(new { command = "loggedIn" });

            var responseQueue = new Queue<string>();
            responseQueue.Enqueue(loginPrompt);
            responseQueue.Enqueue(passwordPrompt);
            responseQueue.Enqueue(loginResponse);

            _mockSocketWrapper.Setup(m => m.Receive(It.IsAny<byte[]>()))
                .Returns((byte[] buffer) =>
                {
                    var response = responseQueue.Dequeue();
                    var bytes = Encoding.ASCII.GetBytes(response);
                    Array.Copy(bytes, buffer, bytes.Length);
                    return bytes.Length;
                });

            // Act
            var result = _userService.Login();

            // Assert
            Assert.True(result);
            _mockSocketWrapper.Verify(m => m.Send(It.Is<byte[]>(b => Encoding.ASCII.GetString(b).Contains(username))), Times.Once);
            _mockSocketWrapper.Verify(m => m.Send(It.Is<byte[]>(b => Encoding.ASCII.GetString(b).Contains(password))), Times.Once);
        }

        [Fact]
        public void Login_ShouldReturnFalse_WhenCredentialsAreIncorrect()
        {
            // Arrange
            var username = "testuser";
            var password = "wrongpassword";
            var loginPrompt = JsonConvert.SerializeObject(new { command = "Enter username:" });
            var passwordPrompt = JsonConvert.SerializeObject(new { command = "Enter password:" });
            var loginResponse = JsonConvert.SerializeObject(new { command = "Incorrect password!" });

            var responseQueue = new Queue<string>();
            responseQueue.Enqueue(loginPrompt);
            responseQueue.Enqueue(passwordPrompt);
            responseQueue.Enqueue(loginResponse);

            _mockSocketWrapper.Setup(m => m.Receive(It.IsAny<byte[]>()))
                .Returns((byte[] buffer) =>
                {
                    var response = responseQueue.Dequeue();
                    var bytes = Encoding.ASCII.GetBytes(response);
                    Array.Copy(bytes, buffer, bytes.Length);
                    return bytes.Length;
                });

            // Act
            var result = _userService.Login();

            // Assert
            Assert.False(result);
            _mockSocketWrapper.Verify(m => m.Send(It.Is<byte[]>(b => Encoding.ASCII.GetString(b).Contains(username))), Times.Once);
            _mockSocketWrapper.Verify(m => m.Send(It.Is<byte[]>(b => Encoding.ASCII.GetString(b).Contains(password))), Times.Once);
        }

        [Fact]
        public void Login_ShouldReturnFalse_WhenUserDoesNotExist()
        {
            // Arrange
            var username = "nonexistentuser";
            var loginPrompt = JsonConvert.SerializeObject(new { command = "Enter username:" });
            var passwordPrompt = JsonConvert.SerializeObject(new { command = "Enter password:" });
            var loginResponse = JsonConvert.SerializeObject(new { command = "User doesn't exist." });

            var responseQueue = new Queue<string>();
            responseQueue.Enqueue(loginPrompt);
            responseQueue.Enqueue(passwordPrompt);
            responseQueue.Enqueue(loginResponse);

            _mockSocketWrapper.Setup(m => m.Receive(It.IsAny<byte[]>()))
                .Returns((byte[] buffer) =>
                {
                    var response = responseQueue.Dequeue();
                    var bytes = Encoding.ASCII.GetBytes(response);
                    Array.Copy(bytes, buffer, bytes.Length);
                    return bytes.Length;
                });

            // Act
            var result = _userService.Login();

            // Assert
            Assert.False(result);
            _mockSocketWrapper.Verify(m => m.Send(It.Is<byte[]>(b => Encoding.ASCII.GetString(b).Contains(username))), Times.Once);
        }
    }