using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Configuration;
using System.Collections.Specialized;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;
using Server.Interfaces;

namespace Server
{
    public class ServerSocket : IServerSocket
    {
        private static Socket clientSocket;
        private readonly static string serverVersion = "0.0.3";
        private readonly static DateTime serverCreationDate = DateTime.Now;
        private byte[] message;
        private string jsonMsg;
        private readonly byte[] bytes = new byte[1024];
        private readonly byte[] bytesU = new byte[1024];
        private readonly byte[] bytesP = new byte[1024];
        private bool loggedIn = false;
        private bool stopped = false;
        private string currentRole;
        private string loggedInUser;
        
        private readonly UserService userService;
        private readonly MessageService messageService;
        

        public ServerSocket(UserService userService, MessageService messageService)
        {
            this.userService = userService;
            this.messageService = messageService;
        }
        
        static void Main(string[] args)
        {
            var userService = new UserService();
            var messageService = new MessageService();
            var serverSocket = new ServerSocket(userService, messageService);
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
                clientSocket = listener.Accept();
                messageService.SetClientSocket(clientSocket);

                Console.WriteLine("Connected");

                while (!stopped)
                {
                    string firstData = ReceiveData();
                    if (firstData == null)
                        break;

                    HandleFirstCommand(firstData.ToLower());

                    while (loggedIn)
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
        
        private void HandleFirstCommand(string command)
        {
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
                { "help", HelpCommand },
                { "info", InfoCommand },
                { "uptime", UptimeCommand },
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
        public void SendData(string message)
        {
            string jsonMsg = JsonConvert.SerializeObject(message);
            byte[] msg = Encoding.ASCII.GetBytes(jsonMsg);
            clientSocket.Send(msg);
        }
        public string ReceiveData()
        {
            byte[] bytes = new byte[1024];
            string jsonData = null;
            int numByte = clientSocket.Receive(bytes);
            jsonData += Encoding.ASCII.GetString(bytes, 0, numByte);
            return JsonConvert.DeserializeObject(jsonData)?.ToString();
        }
        private void PrintUserInfo()
        {

            if (currentRole.ToLower().Equals("admin"))
            {
                jsonMsg = JsonConvert.SerializeObject($"approved");
                message = Encoding.ASCII.GetBytes(jsonMsg);
                clientSocket.Send(message);

                string username;
                User singleUserData = null;
                
                int numByte = clientSocket.Receive(bytesU);
                string jsonUsername = Encoding.ASCII.GetString(bytesU, 0, numByte);
                username = JsonConvert.DeserializeObject(jsonUsername).ToString();

                var file = $"{username}.json";

                if (File.Exists(file))
                {

                    using (StreamReader reader = new StreamReader(file))
                    {
                        var fileRead = File.ReadAllText(file);
                        singleUserData = JsonConvert.DeserializeObject<User>(fileRead);
                    }
                    string getPassword = singleUserData.Password;
                    string getRole = singleUserData.Role;
                    loggedInUser = singleUserData.Userame;
                    jsonMsg = JsonConvert.SerializeObject($"Username: {username}\n" +
                        $"Password: {getPassword}\n" +
                        $"Role: {getRole}\n");
                    byte[] msg = Encoding.ASCII.GetBytes(jsonMsg);
                    clientSocket.Send(msg);
                }
                else
                {
                    jsonMsg = JsonConvert.SerializeObject("User doesn't exist.");
                    byte[] msg = Encoding.ASCII.GetBytes(jsonMsg);
                    clientSocket.Send(msg);
                }


            }
            else
            {
                var file = $"{loggedInUser}.json";
                User singleUserData = null;
                using (StreamReader reader = new StreamReader(file))
                {
                    var fileRead = File.ReadAllText(file);
                    singleUserData = JsonConvert.DeserializeObject<User>(fileRead);
                }
                
                string getPassword = singleUserData.Password;
                currentRole = singleUserData.Role;
                loggedInUser = singleUserData.Userame;

                jsonMsg = JsonConvert.SerializeObject($"Username: {loggedInUser}\n" +
                    $"Password: {getPassword}\n" +
                    $"Role: {currentRole}\n");
                byte[] msg = Encoding.ASCII.GetBytes(jsonMsg);
                clientSocket.Send(msg);
            }
        }
        
        private void Login()
        {
            SendData("Enter username:");
            string username = ReceiveData();
            SendData("Enter password:");
            string password = ReceiveData();

            var (success, message) = userService.Login(username, password);
            SendData(message);

            if (success)
            {
                loggedIn = true;
            }
        }
        private void Logout()
        {
            loggedIn = false;

            jsonMsg = JsonConvert.SerializeObject("logout");
            byte[] message = Encoding.ASCII.GetBytes(jsonMsg);
            clientSocket.Send(message);
        }
        private void AddUser()
        {
            jsonMsg = JsonConvert.SerializeObject($"Enter username:");
            message = Encoding.ASCII.GetBytes(jsonMsg);
            clientSocket.Send(message);

            string username;
            string password;
            int numByte = clientSocket.Receive(bytesU);
            string jsonUsername = Encoding.ASCII.GetString(bytesU, 0, numByte);
            username = JsonConvert.DeserializeObject(jsonUsername).ToString();

            jsonMsg = JsonConvert.SerializeObject($"Enter password:");
            message = Encoding.ASCII.GetBytes(jsonMsg);
            clientSocket.Send(message);

            int numBytePassword = clientSocket.Receive(bytesP);
            string jsonPassword = Encoding.ASCII.GetString(bytesP, 0, numBytePassword);
            password = JsonConvert.DeserializeObject(jsonPassword).ToString();

            string result = userService.AddUser(username, password);

            jsonMsg = JsonConvert.SerializeObject(result);
            message = Encoding.ASCII.GetBytes(jsonMsg);
            clientSocket.Send(message);
        }
        
        private void DeleteUser()
        {
            Console.WriteLine("Enter user (username) to delete: ");
            string username = Console.ReadLine();
            if (File.Exists($"{username}.txt"))
                File.Delete($"{username}.txt");

            jsonMsg = JsonConvert.SerializeObject($"User {username} has been removed.");
            byte[] message = Encoding.ASCII.GetBytes(jsonMsg);
            clientSocket.Send(message);
        }


        private void IncorrectCommand()
        {
            jsonMsg = JsonConvert.SerializeObject($"Incorrect command. Type 'help' to get list of commands.");
            byte[] message = Encoding.ASCII.GetBytes(jsonMsg);
            clientSocket.Send(message);
        }

        public void StopCommand()
        {

            try
            {
                jsonMsg = JsonConvert.SerializeObject("stop");
                byte[] message = Encoding.ASCII.GetBytes(jsonMsg);
                clientSocket.Send(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while sending 'stop' command: " + ex.ToString());
            }
            finally
            {
                // Close the client socket
                //clientSocket.Close();
                loggedIn = false; // Exit the loggedIn loop
                stopped = true;
            }
        }

        private void UptimeCommand()
        {
            DateTime serverCurrentDate = DateTime.Now;
            jsonMsg = JsonConvert.SerializeObject($"Server is up for {serverCurrentDate - serverCreationDate}");
            byte[] message = Encoding.ASCII.GetBytes(jsonMsg);
            clientSocket.Send(message);
        }

        private void InfoCommand()
        {
            jsonMsg = JsonConvert.SerializeObject($"Server version: {serverVersion}\n" +
                                $"Server Creation Date: {serverCreationDate}");
            byte[] message = Encoding.ASCII.GetBytes(jsonMsg);
            clientSocket.Send(message);
        }

        private void HelpCommand()
        {
            jsonMsg = JsonConvert.SerializeObject($"Available commands:\n" +
                                $"'add' - to add new user\n" +
                                $"'help' - to get a list of available commands with their description\n" +
                                $"'info' - to get info about server version, server creation date\n" +
                                $"'msg' - to send a message to other user\n" +
                                $"'read' - to read next message\n" +
                                $"'uptime' - to check server uptime\n" +
                                $"'user' - to print user data" +
                                $"'stop' - to stop the server\n" +
                                $"'logout' - to log out");
            byte[] message = Encoding.ASCII.GetBytes(jsonMsg);
            clientSocket.Send(message);
        }
    }
}