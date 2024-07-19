using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Server.Interfaces;

namespace Server.Services
{
    public class ServerSocket : IServerSocket
    {
        private ISocketWrapper clientSocket;
        private readonly static string serverVersion = "0.0.3";
        private readonly static DateTime serverCreationDate = DateTime.Now;
        private bool stopped = false;
        
        private readonly IUserService userService;
        private readonly IMessageService messageService;
        private readonly IServerInfoService serverInfoService;

        public ServerSocket(IUserService userService, 
            IMessageService messageService, 
            IServerInfoService serverInfoService)
        {
            this.userService = userService;
            this.messageService = messageService;
            this.serverInfoService = serverInfoService;

        }
        
        static void Main(string[] args)
        {
            var userService = new UserService();
            var messageService = new MessageService();
            var serverInfoService = new ServerInfoService(serverVersion, serverCreationDate);
            var serverSocket = new ServerSocket(userService, messageService, serverInfoService);
            serverSocket.StartServer();
        }

        public void StartServer()
        {
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHost.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11111);
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                Console.WriteLine("Awaiting connection...");
                Socket acceptedSocket = listener.Accept();
                clientSocket = new SocketWrapper(acceptedSocket);
                messageService.SetClientSocket(clientSocket);
                serverInfoService.SetClientSocket(clientSocket);


                Console.WriteLine("Connected");

                while (!stopped)
                {
                    string firstData = ReceiveData();
                    if (firstData == null)
                        break;

                    HandleFirstCommand(firstData.ToLower());

                    while (userService.IsLoggedIn())
                    {
                        HandleLoggedInCommands();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                listener.Close();
            }
        }
        
        private void HandleFirstCommand(string commandJson)
        {
            dynamic commandObj = JsonConvert.DeserializeObject(commandJson);
            string command = commandObj.command;
            
            switch (command)
            {
                case "login":
                    Login();
                    break;
                case "add":
                    AddUser();
                    break;
                default:
                    break;
            }
        }

        private void HandleLoggedInCommands()
        {
            SendData("Enter command (type \"help\" to check available commands): ");
            string data = ReceiveData()?.ToLower();
            if (data == null)
                return;

            var commandActions = new Dictionary<string, Action>
            {
                { "add", AddUser },
                { "user", PrintUserInfo },
                { "help", serverInfoService.HelpCommand },
                { "info", serverInfoService.InfoCommand },
                { "uptime", serverInfoService.UptimeCommand },
                { "stop", StopCommand },
                { "logout", Logout },
                { "delete", DeleteUser },
                { "msg", () => messageService.SendMessage() },
                { "read", () => messageService.ReadMessage(userService.GetLoggedInUser()) }
            };

            if (commandActions.ContainsKey(data))
            {
                commandActions[data].Invoke();
            }
            else
            {
                IncorrectCommand();
            }
        }
        private void Login()
        {
            SendData(JsonConvert.SerializeObject(new { message = "Enter username:" }));
            string username = ReceiveData();
            SendData(JsonConvert.SerializeObject(new { message = "Enter password:" }));
            string password = ReceiveData();

            var (success, message) = userService.Login(username, password);
            SendData(JsonConvert.SerializeObject(new { message }));

            if (success)
            {
                // Send a prompt for further commands
                SendData(JsonConvert.SerializeObject(new { message = "Login successful. Awaiting further commands." }));
            }
            else
            {
                SendData(JsonConvert.SerializeObject(new { message = "Login failed." }));
            }
        }

        public void SendData(string message)
        {
            byte[] msg = Encoding.ASCII.GetBytes(message);
            clientSocket.Send(msg);
        }

        public string ReceiveData()
        {
            byte[] bytes = new byte[1024];
            int numByte = clientSocket.Receive(bytes);
            return Encoding.ASCII.GetString(bytes, 0, numByte);
        }

        private void PrintUserInfo()
        {
            string loggedInUser = userService.GetLoggedInUser();
            if (string.IsNullOrEmpty(loggedInUser))
            {
                SendData("No user is currently logged in.");
                return;
            }

            string userInfo = userService.GetUserInfo(loggedInUser);
            SendData(userInfo);
        }

        private void Logout()
        {
            userService.Logout();
            SendData("You have been logged out.");
        }
        private void AddUser()
        {
            SendData("Enter username:");
            string username = ReceiveData();
            SendData("Enter password:");
            string password = ReceiveData();

            string result = userService.AddUser(username, password);
            SendData(result);
        }
        
        private void DeleteUser()
        {
            SendData("Enter username to delete:");
            string username = ReceiveData();

            string result = userService.DeleteUser(username);
            SendData(result);
        }


        private void IncorrectCommand()
        {
            SendData("Incorrect command. Type 'help' to get a list of available commands.");
        }

        private void StopCommand()
        {
            stopped = true;
            SendData("Server stopping...");
        }
    }
}