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

        public bool IsLoggedIn
        {
            get { return isLoggedIn; }
        }

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
            IUserService userService = new UserService();
            IMessageService messageService = new MessageService();
            ClientSocket clientSocket = new ClientSocket(socketWrapper, userService, messageService);

            clientSocket.ExecuteClient(localEndpoint);
        }

        public void ExecuteClient(IPEndPoint localEndpoint)
        {
            try
            {
                _socketWrapper.Connect(localEndpoint);
                Console.WriteLine("Socket connected to -> {0}", localEndpoint.ToString());

                while (continueListening)
                {
                    if (!isLoggedIn)
                    {
                        Menu();
                    }

                    while (isLoggedIn)
                    {
                        // Whole method needs to be changed

                        //------------START-------------
                        
                        string commandPrompt = ReceiveJsonData();
                        Console.WriteLine(commandPrompt);

                        string command = Console.ReadLine();
                        switch (command)
                        {
                            case "add":
                                AddUser();
                                break;
                            case "logout":
                                Logout(command);
                                break;
                            case "stop":
                                Stop(command);
                                break;
                            case "msg":
                                //_messageService.SendMessage(_socketWrapper);
                                break;
                            case "read":
                                //_messageService.ReadMessage(_socketWrapper, _userService.GetLoggedInUser());
                                break;
                            case "user":
                                PrintUserInfo(command);
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

        private void Menu()
        {
            Console.WriteLine("\nType '1' to login\nType '2' to create new user\n");
            char choice = Console.ReadKey().KeyChar;
            Console.WriteLine();
            if (choice == '1')
            {
                SendInitialCommand("login");
                Login();
            }
            else if (choice == '2')
            {
                SendInitialCommand("add");
                AddUser();
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
            string jsonCommand = JsonConvert.SerializeObject(new { command });
            byte[] messageSent = Encoding.ASCII.GetBytes(jsonCommand);
            _socketWrapper.Send(messageSent);

            string jsonResponse = ReceiveJsonData();
            Console.WriteLine(jsonResponse);
        }

        private void Stop(string command)
        {
            string jsonCommand = JsonConvert.SerializeObject(command);
            byte[] messageSent = Encoding.ASCII.GetBytes(jsonCommand);

            _socketWrapper.Send(messageSent);
            continueListening = false;
        }

        public void Login()
        {
            string usernamePrompt = ReceiveJsonData();
            Console.WriteLine(usernamePrompt);

            string username = Console.ReadLine();
            SendData(username);

            string passwordPrompt = ReceiveJsonData();
            Console.WriteLine(passwordPrompt);

            string password = Console.ReadLine();
            SendData(password);

            string jsonResponse = ReceiveJsonData();
            dynamic response = JsonConvert.DeserializeObject(jsonResponse);
            Console.WriteLine(response.message);

            if (response.message == "loggedIn")
            {
                Console.WriteLine("\nLogin successful. Awaiting further commands.");
                isLoggedIn = true;
            }
        }

        private void Logout(string command)
        {
            _userService.Logout();
            isLoggedIn = false;
            Console.WriteLine("You have been logged out.");
        }

        private void PrintUserInfo(string command)
        {
            string jsonCommand = JsonConvert.SerializeObject(command);
            byte[] messageSent = Encoding.ASCII.GetBytes(jsonCommand);
            _socketWrapper.Send(messageSent);

            byte[] messageReceived = new byte[1024];
            int byteRcvd = _socketWrapper.Receive(messageReceived);
            string jsonString = Encoding.ASCII.GetString(messageReceived, 0, byteRcvd);
            string encodingString = JsonConvert.DeserializeObject(jsonString).ToString();

            if (encodingString.ToLower().Equals("approved"))
            {
                Console.WriteLine("\nEnter username you'd like to check");
                string username = Console.ReadLine();
                DefaultMessage(username);
            }
            else
            {
                DefaultMessage(encodingString);
            }

            
        }

        private void AddUser()
        {
            Console.WriteLine("Enter username:");
            string username = Console.ReadLine();
            Console.WriteLine("Enter password:");
            string password = Console.ReadLine();

            string result = _userService.AddUser(username, password);
            Console.WriteLine(result);
        }
        
        private string ReceiveJsonData()
        {
            byte[] buffer = new byte[1024];
            int bytesReceived = _socketWrapper.Receive(buffer);
            string jsonString = Encoding.ASCII.GetString(buffer, 0, bytesReceived);
            dynamic response = JsonConvert.DeserializeObject(jsonString);
            return response.message;
        }
        private void SendData(string data)
        {
            string jsonData = JsonConvert.SerializeObject(new { message = data });
            byte[] messageSent = Encoding.ASCII.GetBytes(jsonData);
            _socketWrapper.Send(messageSent);
        }
    }
}