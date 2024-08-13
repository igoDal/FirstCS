using Moq;
using Xunit;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Server.Interfaces;
using Server.Services;

namespace Server.Tests
{
    public class ServerSocketTests
    {
        private readonly UserService _userService;
        private readonly Mock<ISocketWrapper> _mockSocketWrapper;
        private readonly Mock<IUserService> _mockUserService;
        private readonly ServerSocket _serverSocket;

        public ServerSocketTests()
        {
            _userService = new UserService();
            _mockSocketWrapper = new Mock<ISocketWrapper>();
            _mockUserService = new Mock<IUserService>();

            _serverSocket = new ServerSocket(
                _mockUserService.Object,
                new Mock<IMessageService>().Object,
                new Mock<IServerInfoService>().Object
            );

            typeof(ServerSocket).GetField("clientSocket", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_serverSocket, _mockSocketWrapper.Object);
        }
        [Fact]
        public void PrintUserInfo_ShouldSendApproved_WhenUserIsAdmin()
        {
            // Arrange
            var username = "adminUser";
            var userInfo = "User Info: adminUser";
            var command = "approved";

            _mockUserService.Setup(m => m.GetLoggedInUser()).Returns(username);
            _mockUserService.Setup(m => m.GetCurrentRole()).Returns("admin");
            _mockUserService.Setup(m => m.GetUserInfo(It.IsAny<string>())).Returns(userInfo);

            _mockSocketWrapper.Setup(m => m.Receive(It.IsAny<byte[]>()))
                .Callback<byte[]>(buffer =>
                {
                    var jsonString = JsonConvert.SerializeObject(new { command = username });
                    var bytes = Encoding.ASCII.GetBytes(jsonString);
                    bytes.CopyTo(buffer, 0);
                })
                .Returns((byte[] buffer) => Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(new { command = username })).Length);

            // Act
            _serverSocket.PrintUserInfo();

            // Assert
            _mockSocketWrapper.Verify(m => m.Send(It.Is<byte[]>(b => Encoding.ASCII.GetString(b).Contains(command))), Times.Once);
            _mockSocketWrapper.Verify(m => m.Send(It.Is<byte[]>(b => Encoding.ASCII.GetString(b).Contains(userInfo))), Times.Once);
        }
        [Fact]
        public void PrintUserInfo_ShouldSendNotApproved_WhenUserIsNotAdmin()
        {
            // Arrange
            var username = "regularUser";
            var userInfo = "User Info: regularUser";
            var command = "not approved";

            _mockUserService.Setup(m => m.GetLoggedInUser()).Returns(username);
            _mockUserService.Setup(m => m.GetCurrentRole()).Returns("user");
            _mockUserService.Setup(m => m.GetUserInfo(It.IsAny<string>())).Returns(userInfo);

            _mockSocketWrapper.Setup(m => m.Receive(It.IsAny<byte[]>()))
                .Callback<byte[]>(buffer =>
                {
                    var jsonString = JsonConvert.SerializeObject(new { command = username });
                    var bytes = Encoding.ASCII.GetBytes(jsonString);
                    bytes.CopyTo(buffer, 0);
                })
                .Returns((byte[] buffer) => Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(new { command = username })).Length);

            // Act
            _serverSocket.PrintUserInfo();

            // Assert
            _mockSocketWrapper.Verify(m => m.Send(It.Is<byte[]>(b => Encoding.ASCII.GetString(b).Contains(command))), Times.Once);
            _mockSocketWrapper.Verify(m => m.Send(It.Is<byte[]>(b => Encoding.ASCII.GetString(b).Contains(userInfo))), Times.Once);
        }

        [Fact]
        public void DeleteUser_ShouldDeleteExistingUser()
        {
            // Arrange
            var username = "usertodelete";
            var password = "password123";
            var expectedFilePath = $"{username}.json";

            _userService.AddUser(username, password);

            // Act
            var result = _userService.DeleteUser(username);

            // Assert
            Assert.Equal($"User {username} has been deleted.", result);
            Assert.False(File.Exists(expectedFilePath));
        }
        
        [Fact]
        public void DeleteUser_ShouldNotDeleteNonExistingUser()
        {
            // Arrange
            var username = "nonexistinguser";
            var expectedFilePath = $"{username}.json";

            // Act
            var result = _userService.DeleteUser(username);

            // Assert
            Assert.Equal($"User {username} does not exist.", result);
            Assert.False(File.Exists(expectedFilePath));
        }
        
    }
}