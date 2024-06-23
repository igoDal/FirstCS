using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Server;
using Xunit;

namespace Server.Tests
{
    public class ServerSocketTests : IDisposable
    {
        private ServerSocket serverSocket;
        private Socket clientSocket;

        public ServerSocketTests()
        {
            serverSocket = new ServerSocket();
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Dispose()
        {
            clientSocket.Close();
        }

        [Fact]
        public void TestServerCreation()
        {
            Assert.NotNull(serverSocket);
        }

        [Fact]
        public void TestLogin()
        {
            string username = "testuser";
            string password = "testpassword";
            string userData = JsonConvert.SerializeObject(new User() { Userame = username, Password = password, Role = "user" });
            File.WriteAllText($"{username}.json", userData);

            var jsonUsername = JsonConvert.SerializeObject(username);
            var jsonPassword = JsonConvert.SerializeObject(password);

            clientSocket.Connect(IPAddress.Loopback, 11111);
            clientSocket.Send(Encoding.ASCII.GetBytes(jsonUsername));
            clientSocket.Send(Encoding.ASCII.GetBytes(jsonPassword));

            byte[] responseBytes = new byte[1024];
            int numByte = clientSocket.Receive(responseBytes);
            string response = Encoding.ASCII.GetString(responseBytes, 0, numByte);

            Assert.Equal("loggedIn", JsonConvert.DeserializeObject<string>(response));

            File.Delete($"{username}.json");
        }

        [Fact]
        public void TestAddUser()
        {
            string username = "newuser";
            string password = "newpassword";
            var jsonUsername = JsonConvert.SerializeObject(username);
            var jsonPassword = JsonConvert.SerializeObject(password);

            clientSocket.Connect(IPAddress.Loopback, 11111);
            clientSocket.Send(Encoding.ASCII.GetBytes(jsonUsername));
            clientSocket.Send(Encoding.ASCII.GetBytes(jsonPassword));

            byte[] responseBytes = new byte[1024];
            int numByte = clientSocket.Receive(responseBytes);
            string response = Encoding.ASCII.GetString(responseBytes, 0, numByte);

            Assert.Equal($"User {username} has been added.", JsonConvert.DeserializeObject<string>(response));

            File.Delete($"{username}.json");
        }

    }
}
