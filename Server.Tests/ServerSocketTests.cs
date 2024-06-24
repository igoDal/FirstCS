using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Server;
using Xunit;

namespace Server.Tests
{
    public class ServerSocketTests : IDisposable
    {
        private Thread serverThread;
        private Socket clientSocket;

        public ServerSocketTests()
        {
            serverThread = new Thread(StartServer);
            serverThread.Start();
            Thread.Sleep(1000); 

            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        private void StartServer()
        {
            ServerSocket serverSocket = new ServerSocket();
            serverSocket.ExecuteServer();
        }

        public void Dispose()
        {
            clientSocket.Close();
            serverThread.Abort();
        }

        [Fact]
        public void TestServerCreation()
        {
            Assert.NotNull(serverThread);
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

            ConnectClient();
            SendData(clientSocket, jsonUsername);
            SendData(clientSocket, jsonPassword);

            string response = ReceiveData(clientSocket);

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

            ConnectClient();
            SendData(clientSocket, jsonUsername);
            SendData(clientSocket, jsonPassword);

            string response = ReceiveData(clientSocket);

            Assert.Equal($"User {username} has been added.", JsonConvert.DeserializeObject<string>(response));

            File.Delete($"{username}.json"); 
        }

        private void ConnectClient()
        {
            if (!clientSocket.Connected)
            {
                clientSocket.Connect(IPAddress.Loopback, 11111);
            }
        }

        private void SendData(Socket socket, string data)
        {
            byte[] message = Encoding.ASCII.GetBytes(data);
            socket.Send(message);
        }

        private string ReceiveData(Socket socket)
        {
            byte[] buffer = new byte[1024];
            int bytesReceived = socket.Receive(buffer);
            return Encoding.ASCII.GetString(buffer, 0, bytesReceived);
        }

    }
}
