using System.Text;
using Client.Interfaces;
using Moq;
using Newtonsoft.Json;

namespace Client.Test;

public class ClientSocketTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<ISocketWrapper> _socketWrapperMock;
        private readonly ClientSocket _clientSocket;

        public ClientSocketTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _socketWrapperMock = new Mock<ISocketWrapper>();
            _clientSocket = new ClientSocket(_socketWrapperMock.Object, _userServiceMock.Object, Mock.Of<IMessageService>());
        }

        [Fact]
        public void PrintUserInfo_ShouldSendUserInfo_WhenUserIsLoggedIn()
        {
            // Arrange
            var username = "testuser";
            var userInfo = $"Username: {username}\nPassword: password123\nRole: user";

            _userServiceMock.Setup(us => us.GetLoggedInUser()).Returns(username);
            _userServiceMock.Setup(us => us.GetUserInfo(username)).Returns(userInfo);

            var command = "user";
            var jsonCommand = JsonConvert.SerializeObject(new { command });
            var jsonResponse = JsonConvert.SerializeObject(new { message = "approved" });

            _socketWrapperMock.Setup(sw => sw.Receive(It.IsAny<byte[]>())).Returns(Encoding.ASCII.GetBytes(jsonResponse));
            _socketWrapperMock.Setup(sw => sw.Send(It.IsAny<byte[]>()));

            // Act
            _clientSocket.PrintUserInfo(command);

            // Assert
            _socketWrapperMock.Verify(sw => sw.Send(It.Is<byte[]>(b => Encoding.ASCII.GetString(b).Contains(userInfo))), Times.Once);
        }

        [Fact]
        public void PrintUserInfo_ShouldSendErrorMessage_WhenNoUserIsLoggedIn()
        {
            // Arrange
            var command = "user";
            var jsonCommand = JsonConvert.SerializeObject(new { command });
            var jsonResponse = JsonConvert.SerializeObject(new { message = "No user is currently logged in." });

            _userServiceMock.Setup(us => us.GetLoggedInUser()).Returns((string)null);
            _socketWrapperMock.Setup(sw => sw.Receive(It.IsAny<byte[]>())).Returns(Encoding.ASCII.GetBytes(jsonResponse));
            _socketWrapperMock.Setup(sw => sw.Send(It.IsAny<byte[]>()));

            // Act
            _clientSocket.PrintUserInfo(command);

            // Assert
            _socketWrapperMock.Verify(sw => sw.Send(It.Is<byte[]>(b => Encoding.ASCII.GetString(b).Contains("No user is currently logged in."))), Times.Once);
        }
        
        [Fact]
        public void AddUser_ShouldSendSuccessMessage_WhenUserIsAddedSuccessfully()
        {
            // Arrange
            var username = "newuser";
            var password = "password123";
            var result = $"User {username} has been added.";

            _userServiceMock.Setup(us => us.AddUser(username, password)).Returns(result);
            _socketWrapperMock.Setup(sw => sw.Send(It.IsAny<byte[]>()));

            // Act
            _clientSocket.AddUser(username, password);

            // Assert
            _socketWrapperMock.Verify(sw => sw.Send(It.Is<byte[]>(b => Encoding.ASCII.GetString(b).Contains(result))), Times.Once);
        }

        [Fact]
        public void AddUser_ShouldSendErrorMessage_WhenUserAlreadyExists()
        {
            // Arrange
            var username = "existinguser";
            var password = "password123";
            var result = $"User {username} already exists.";

            _userServiceMock.Setup(us => us.AddUser(username, password)).Returns(result);
            _socketWrapperMock.Setup(sw => sw.Send(It.IsAny<byte[]>()));

            // Act
            _clientSocket.AddUser(username, password);

            // Assert
            _socketWrapperMock.Verify(sw => sw.Send(It.Is<byte[]>(b => Encoding.ASCII.GetString(b).Contains(result))), Times.Once);
        }
    }