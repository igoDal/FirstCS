using Moq;
using Xunit;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Server.Services;

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
            var username = "123aaa";
            var password = "password123";
            var expectedFilePath = $"{username}.json";

            // Act
            var result = _userService.AddUser(username, password);

            // Assert
            Assert.Equal($"User {username} has been added.", result);
            Assert.True(File.Exists(expectedFilePath));
            
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
            
            _userService.AddUser(username, password);

            // Act
            var result = _userService.AddUser(username, password);

            // Assert
            Assert.Equal($"User {username} already exists.", result);
            
            if (File.Exists(expectedFilePath))
            {
                File.Delete(expectedFilePath);
            }
        }
    }
}