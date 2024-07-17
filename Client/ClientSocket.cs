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
        private bool notLoggedInFlag = false;
        private ISocketWrapper _socketWrapper;
        private readonly IUserService _userService;
        public bool IsLoggedIn
        {
            get { return isLoggedIn; }
        }

        public ClientSocket(ISocketWrapper socketWrapper, IUserService userService)
        {
            _socketWrapper = socketWrapper;
            _userService = userService;
        }

        static void Main(string[] args)
        {
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHost.AddressList[0];
            IPEndPoint localEndpoint = new IPEndPoint(ipAddress, 11111);
            Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            ISocketWrapper socketWrapper = new SocketWrapper(socket);
            IUserService userService = new UserService();
            ClientSocket clientSocket = new ClientSocket(socketWrapper, userService);

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
                    Menu();

                    while (isLoggedIn)
                    {
                        // Whole method needs to be changed

                        //------------START-------------
                        
                        byte[] initialCommand = new byte[1024];
                        int initComm = _socketWrapper.Receive(initialCommand);
                        string jsonInitComm = Encoding.ASCII.GetString(initialCommand, 0, initComm);
                        string encodingInitComm = JsonConvert.DeserializeObject(jsonInitComm).ToString();

                        Console.WriteLine(encodingInitComm);

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
                                SendMessage(command);
                                break;
                            case "read":
                                ReadMessage(command);
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

        private void ReadMessage(string command)
        {
            DefaultMessage(command);
        }

        public void SendMessage(string command)
        {
            //Call sendMessage method on server side
            string jsonCommand = JsonConvert.SerializeObject(command);
            byte[] msgCommand = Encoding.ASCII.GetBytes(jsonCommand);
            _socketWrapper.Send(msgCommand);
            
            byte[] msgReceived = new byte[1024];
            int byteRcvd = _socketWrapper.Receive(msgReceived);
            string jsonString = Encoding.ASCII.GetString(msgReceived, 0, byteRcvd);
            string encodingString;
            try
            {
                var deserializedObject = JsonConvert.DeserializeObject(jsonString);
                encodingString = deserializedObject.ToString();
            }
            catch (JsonReaderException)
            {
                encodingString = jsonString;
            }

            Console.WriteLine(encodingString);
            
            //Request for username (message receiver)
            string userToSend = Console.ReadLine();
            string jsonUserToSend= JsonConvert.SerializeObject(userToSend);
            byte[] usernameSent = Encoding.ASCII.GetBytes(jsonUserToSend);
            _socketWrapper.Send(usernameSent);

            byte[] userToSendReceived = new byte[1024];
            int byteUserRcvd = _socketWrapper.Receive(userToSendReceived);
            string jsonUserString = Encoding.ASCII.GetString(userToSendReceived, 0, byteUserRcvd);
            string encodingUserString = JsonConvert.DeserializeObject(jsonUserString).ToString();
            Console.WriteLine(encodingUserString);

            string message = Console.ReadLine();
            const int MAX_LENGTH = 255;
            if (message.Length > MAX_LENGTH)
            {
                message = message.Substring(0, MAX_LENGTH);
            }
            string jsonMessage = JsonConvert.SerializeObject(message);
            byte[] messageToSend = Encoding.ASCII.GetBytes(jsonMessage);
            _socketWrapper.Send(messageToSend);

            byte[] messageReceived = new byte[1024];
            int byteMessageRcvd = _socketWrapper.Receive(messageReceived);
            string jsonStringMessage = Encoding.ASCII.GetString(messageReceived, 0, byteMessageRcvd);
            string encodingStringMessage = JsonConvert.DeserializeObject(jsonStringMessage).ToString();
            Console.WriteLine(encodingStringMessage);
        }

        private void Menu()
        {
            Console.WriteLine("\nType '1' to login\n" +
                            "Type '2' to create new user\n"); //+
            char choice = Console.ReadKey().KeyChar;
            Console.WriteLine();
            if (choice == '1')
            {
                Console.WriteLine("\nPodaj login: ");
                string username = Console.ReadLine();
                Console.WriteLine("\nPodaj hasło: ");
                string password = Console.ReadLine();
                Login(username, password);
            }
            else if (choice == '2')
            {
                AddUser();
            }
            else
            {
                return;
            }
        }

        private void DefaultMessage(string command)
        {
            string jsonCommand = JsonConvert.SerializeObject(command);
            byte[] messageSent = Encoding.ASCII.GetBytes(jsonCommand);
            _socketWrapper.Send(messageSent);

            byte[] messageReceived = new byte[1024];

            int byteRcvd = _socketWrapper.Receive(messageReceived);
            string jsonString = Encoding.ASCII.GetString(messageReceived, 0, byteRcvd);
            string encodingString = JsonConvert.DeserializeObject(jsonString).ToString();
            Console.WriteLine(encodingString);
        }

        private void Stop(string command)
        {
            string jsonCommand = JsonConvert.SerializeObject(command);
            byte[] messageSent = Encoding.ASCII.GetBytes(jsonCommand);

            _socketWrapper.Send(messageSent);
            continueListening = false;
        }

        public void Login(string username, string password)
        {
            var (success, message) = _userService.Login(username, password);
            Console.WriteLine(message);

            if (success)
            {
                Console.WriteLine($"\n{username} has logged in.");
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

        private string UsernameRequest(string command)
        {
            string jsonCommand = JsonConvert.SerializeObject(command);
            byte[] messageSentUsername = Encoding.ASCII.GetBytes(jsonCommand);
            _socketWrapper.Send(messageSentUsername);
            
            byte[] messageReceivedUser = new byte[1024];
            int byteRcvdUser = _socketWrapper.Receive(messageReceivedUser);
            string jsonString = Encoding.ASCII.GetString(messageReceivedUser, 0, byteRcvdUser);

            try
            {
                var deserializedObject = JsonConvert.DeserializeObject(jsonString);
                return deserializedObject.ToString();
            }
            catch (JsonReaderException)
            {
                return jsonString;
            }
        }

        private void EnterUsername(string username)
        {
            string jsonSendUsername = JsonConvert.SerializeObject(username);
            byte[] sendUsername = Encoding.ASCII.GetBytes(jsonSendUsername);
            _socketWrapper.Send(sendUsername);
        }

        private string PasswordRequest()
        {
            byte[] receivePasswordRequest = new byte[1024];
            int passwordRequestReceived = _socketWrapper.Receive(receivePasswordRequest);
            string jsonStringPasswordRequest = Encoding.ASCII.GetString(receivePasswordRequest, 0, passwordRequestReceived);

            string encodingStringPasswordRequest;
            try
            {
                var deserializedObject = JsonConvert.DeserializeObject(jsonStringPasswordRequest);
                encodingStringPasswordRequest =  deserializedObject.ToString();
            }
            catch (JsonReaderException)
            {
                encodingStringPasswordRequest = jsonStringPasswordRequest;
            }
            if (encodingStringPasswordRequest.ToLower().Equals("user doesn't exist."))
            {
                notLoggedInFlag = true;
            }

            return encodingStringPasswordRequest;
        }

        private void EnterPassword(string password)
        {
            string jsonSendPassword = JsonConvert.SerializeObject(password);
            byte[] sendPassword = Encoding.ASCII.GetBytes(jsonSendPassword);
            _socketWrapper.Send(sendPassword);
        }
    }
}