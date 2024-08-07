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