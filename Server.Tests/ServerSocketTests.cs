using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;

namespace Server.Tests
{
    public class ServerSocketTests
    {
        [Fact]
        public async Task InfoCommand_ShouldSendCorrectServerInfo()
        {
            // Arrange
            var serverSocketTask = Task.Run(() => StartServer());

            // Wait a bit for the server to start
            await Task.Delay(500);

            var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(IPAddress.Loopback, 11111);

            var jsonCommand = JsonConvert.SerializeObject("info");
            var commandBytes = Encoding.ASCII.GetBytes(jsonCommand);

            // Act
            clientSocket.Send(commandBytes);
            
            var buffer = new byte[1024];
            int bytesRead = clientSocket.Receive(buffer);
            var response = Encoding.ASCII.GetString(buffer, 0, bytesRead);

            // Assert
            var expectedMessage = $"Server version: 0.0.3\n" + // Adjust based on your actual version
                                  $"Server Creation Date: ";
            var expectedJsonMsg = JsonConvert.SerializeObject(expectedMessage);
            var expectedPrefix = expectedJsonMsg.Substring(0, expectedJsonMsg.LastIndexOf("Creation Date: ") + "Creation Date: ".Length);

            Assert.StartsWith(expectedPrefix, response);

            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }

        private void StartServer()
        {
            var server = new ServerSocket();
            server.ExecuteServer();
        }
    }
}