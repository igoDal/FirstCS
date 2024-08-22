using System.Configuration;
using System.Collections.Specialized;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Client.Interfaces;
using Client.Services;
using Newtonsoft.Json;

namespace Client
{
    public class ClientSocket
    {
        private bool isLoggedIn = false;
        private bool continueListening = true;
        private ISocketWrapper _socketWrapper;
        private readonly IUserService _userService;
        private readonly IMessageService _messageService;

        public bool IsLoggedIn => _userService.IsLoggedIn;

        public ClientSocket(ISocketWrapper socketWrapper, IUserService userService, IMessageService messageService)
        {
            _socketWrapper = socketWrapper;
            _userService = userService;
            _messageService = messageService;

        }

        static void Main(string[] args)
        {
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHost.AddressList[0];
            IPEndPoint localEndpoint = new IPEndPoint(ipAddress, 11111);
            Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            ISocketWrapper socketWrapper = new SocketWrapper(socket);
            IUserService userService = new UserService(socketWrapper);
            IMessageService messageService = new MessageService();
            ClientSocket clientSocket = new ClientSocket(socketWrapper, userService, messageService);

            clientSocket.ExecuteClient(localEndpoint);
        }

        public void ExecuteClient(IPEndPoint localEndpoint)
        {
            try
            {
                _socketWrapper.Connect(localEndpoint);
                _messageService.SetClientSocket(_socketWrapper);
                Console.WriteLine("Socket connected to -> {0}", localEndpoint.ToString());

                while (continueListening)
                {
                    if (!isLoggedIn)
                    {
                        Menu();
                    }

                    while (isLoggedIn)
                    {
                        string commandPrompt = ReceiveJsonData();
                        Console.WriteLine(commandPrompt);

                        string command = Console.ReadLine();
                        switch (command)
                        {
                            case "add":
                                _userService.AddUser();
                                break;
                            case "logout":
                                Logout(command);
                                break;
                            case "stop":
                                Stop(command);
                                break;
                            case "msg":
                                _messageService.SendMessage(_socketWrapper);
                                break;
                            case "read":
                                _messageService.ReadMessage(_socketWrapper);
                                break;
                            case "user":
                                _userService.PrintUserInfo(command);
                                break;
                            default:
                                DefaultMessage(command);
                                break;
                        }
                        if (!continueListening)
                            break;
                    }
                }
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException ; {0}", ane.ToString());
            }

            catch (SocketException se)
            {
                Console.WriteLine("Socketxception : {0}", se.ToString());
            }

            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }
        }

        public void Menu()
        {
            Console.WriteLine("\nType '1' to login\nType '2' to create new user\n");
            var choice = Console.ReadLine();
            Console.WriteLine();
            if (choice == "1")
            {
                SendInitialCommand("login");
                bool loginSuccess = _userService.Login();
                if (loginSuccess)
                {
                    isLoggedIn = true;
                }
            }
            else if (choice == "2")
            {
                SendInitialCommand("add");
                _userService.AddUser();
            }
            else
            {
                return;
            }
        }
        private void SendInitialCommand(string command)
        {
            string jsonCommand = JsonConvert.SerializeObject(new { command });
            byte[] messageSent = Encoding.ASCII.GetBytes(jsonCommand);
            _socketWrapper.Send(messageSent);
        }
        private void DefaultMessage(string command)
        {
            SendData(command);

            string jsonResponse = ReceiveJsonData();
            Console.WriteLine(jsonResponse);
        }

        public void Stop(string command)
        {
            string jsonCommand = JsonConvert.SerializeObject(new { command = command });
            byte[] messageSent = Encoding.ASCII.GetBytes(jsonCommand);

            _socketWrapper.Send(messageSent);

            byte[] buffer = new byte[1024];
            int numByte = _socketWrapper.Receive(buffer);
            string response = Encoding.ASCII.GetString(buffer, 0, numByte);

            if (response.Contains("Server stopping..."))
            {
                _socketWrapper.Close();
                continueListening = false;
                Console.WriteLine("Client socket closed, exiting application.");
            }
        }
        
        public void Logout(string command)
        {
            _userService.Logout();
            isLoggedIn = false;
            Console.WriteLine("You have been logged out.");
        }

        
        
        public string ReceiveJsonData()
        {
            byte[] buffer = new byte[1024];
            int bytesReceived = _socketWrapper.Receive(buffer);
            string jsonString = Encoding.ASCII.GetString(buffer, 0, bytesReceived);
            dynamic response = JsonConvert.DeserializeObject(jsonString);
            return response.command;
        }
        public void SendData(string data)
        {
            string jsonData = JsonConvert.SerializeObject(new { command = data });
            byte[] messageSent = Encoding.ASCII.GetBytes(jsonData);
            _socketWrapper.Send(messageSent);
        }
    }
}