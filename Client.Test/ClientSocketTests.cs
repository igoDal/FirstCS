using System.Text;
using Client.Interfaces;
using Client.Services;
using Moq;
using Newtonsoft.Json;

namespace Client.Test;

public class ClientSocketTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<ISocketWrapper> _mockSocketWrapper;
        private readonly ClientSocket _clientSocket;
        private readonly UserService _userService;


        public ClientSocketTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockSocketWrapper = new Mock<ISocketWrapper>();
            _clientSocket = new ClientSocket(_mockSocketWrapper.Object, _mockUserService.Object, Mock.Of<IMessageService>());
            _userService = new UserService(_mockSocketWrapper.Object);
        }
        [Fact]
        public void Menu_ShouldTriggerLogin_WhenOption1IsSelected()
        {
            // Arrange
            var input = new StringReader("1\n");
            Console.SetIn(input);

            // Act
            _clientSocket.Menu();

            // Assert
            _mockUserService.Verify(us => us.Login(), Times.Once);
            _mockSocketWrapper.Verify(sw => sw.Send(It.Is<byte[]>(b => Encoding.ASCII.GetString(b).Contains("login"))), Times.Once);
        }
        
        [Fact]
        public void Menu_ShouldDoNothing_WhenInvalidOptionIsSelected()
        {
            // Arrange
            var input = new StringReader("invalid\n");
            Console.SetIn(input);

            // Act
            _clientSocket.Menu();

            // Assert
            _mockUserService.Verify(us => us.Login(), Times.Never);
            _mockUserService.Verify(us => us.AddUser(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockSocketWrapper.Verify(sw => sw.Send(It.IsAny<byte[]>()), Times.Never);
        }

    }