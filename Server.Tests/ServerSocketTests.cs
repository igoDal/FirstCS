using Moq;
using Xunit;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Server.Tests
{
    public class ServerSocketTests
    {
        private readonly UserService _userService;

        public ServerSocketTests()
        {
            _userService = new UserService();
        }

        [Fact]
        public void AddUser_ShouldAddNewUser()
        {
            // Arrange
            var username = "testuser";
            var password = "password123";
            var expectedFilePath = $"{username}.json";

            // Act
            var result = _userService.AddUser(username, password);

            // Assert
            Assert.Equal($"User {username} has been added.", result);
            Assert.True(File.Exists(expectedFilePath));

            // Cleanup
            if (File.Exists(expectedFilePath))
            {
                File.Delete(expectedFilePath);
            }
        }

        [Fact]
        public void AddUser_ShouldNotAddExistingUser()
        {
            // Arrange
            var username = "existinguser";
            var password = "password123";
            var expectedFilePath = $"{username}.json";

            // Add user first time
            _userService.AddUser(username, password);

            // Act
            var result = _userService.AddUser(username, password);

            // Assert
            Assert.Equal($"User {username} already exists.", result);

            // Cleanup
            if (File.Exists(expectedFilePath))
            {
                File.Delete(expectedFilePath);
            }
        }
    }
}