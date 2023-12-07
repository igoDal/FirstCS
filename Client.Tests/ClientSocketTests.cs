//using Xunit;

//namespace Client.Tests
//{
//    public class UnitTest1
//    {
//        [Fact]
//        public void Login_ValidCredentials_ReturnsTrue()
//        {
//            // Arrange
//            var clientSocket = new ClientSocket();

//            // Act
//            bool result = clientSocket.login("valid_username", "valid_password");

//            // Assert
//            Assert.True(result);
//        }

//        [Fact]
//        public void Login_InvalidCredentials_ReturnsFalse()
//        {
//            // Arrange
//            var clientSocket = new ClientSocket();

//            // Act
//            bool result = clientSocket.login("invalid_username", "invalid_password");

//            // Assert
//            Assert.False(result);
//        }

//        [Fact]
//        public void DefaultMessage_ReturnsExpectedMessage()
//        {
//            // Arrange
//            var clientSocket = new ClientSocket();

//            // Act
//            string message = clientSocket.DefaultMessage();

//            // Assert
//            Assert.Equal("Welcome to the server!", message);
//        }
//    }
//}