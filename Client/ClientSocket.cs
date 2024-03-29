﻿using System.Configuration;
using System.Collections.Specialized;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace Client
{
    public class ClientSocket
    {
        private  bool isLoggedIn = false;
        private static bool isOnline = false;
        private static bool continueListening = true;
        private static Socket sender;
        private static bool notLoggedInFlag = false;
        private readonly IRealClientSocket _client;

        public bool IsLoggedIn
        {
            get { return isLoggedIn; }
        }

        public ClientSocket(IRealClientSocket client)
        {
            _client = client;
        }

        static void Main(string[] args)
        {
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHost.AddressList[0];
            IPEndPoint localEndpoint = new IPEndPoint(ipAddress, 11111);
            sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            IRealClientSocket realSocketClient = new RealClientSocket(sender);
            ClientSocket clientSocket = new ClientSocket(realSocketClient);


            clientSocket.ExecuteClient(sender, localEndpoint);
        }

        void ExecuteClient(Socket sender, IPEndPoint localEndpoint)
        {
            try
            {
                try
                {
                    sender.Connect(localEndpoint);
                    Console.WriteLine("Socket connected to -> {0}", sender.RemoteEndPoint.ToString());

                    while (continueListening)
                    {
                        Menu();

                        while (isLoggedIn)
                        {
                            // Whole method needs to be changed

                            //------------START-------------
                            
                            byte[] initialCommand = new byte[1024];

                            int initComm = sender.Receive(initialCommand);

                            string jsonInitComm = Encoding.ASCII.GetString(initialCommand, 0, initComm);
                            string encodingInitComm = JsonConvert.DeserializeObject(jsonInitComm).ToString();

                            Console.WriteLine(encodingInitComm);

                            string command = Console.ReadLine();
                            switch (command)
                            {
                                case "add":
                                    addUser();
                                    break;
                                case "logout":
                                    logout(command);
                                    break;
                                case "stop":
                                    stop(command);
                                    break;
                                case "msg":
                                    sendMessage(command);
                                    break;
                                case "read":
                                    readMessage(command);
                                    break;
                                case "user":
                                    printUserInfo(command);
                                    break;
                                default:
                                    defaultMessage(command);
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
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            sender.Close();
        }

        private void readMessage(string command)
        {
            defaultMessage(command);
        }

        private void sendMessage(string command)
        {
            //Call sendMessage method on server side
            string jsonCommand = JsonConvert.SerializeObject(command);
            byte[] msgCommand = Encoding.ASCII.GetBytes(jsonCommand);
            int byteSent = sender.Send(msgCommand);
            byte[] msgReceived = new byte[1024];
            int byteRcvd = sender.Receive(msgReceived);
            string jsonString = Encoding.ASCII.GetString(msgReceived, 0, byteRcvd);
            string encodingString = JsonConvert.DeserializeObject(jsonString).ToString();

            Console.WriteLine(encodingString);
            
            //Request for username (message receiver)
            string userToSend = Console.ReadLine();
            string jsonUserToSend= JsonConvert.SerializeObject(userToSend);
            byte[] usernameSent = Encoding.ASCII.GetBytes(jsonUserToSend);
            int byteUserToSend = sender.Send(usernameSent);

            byte[] userToSendReceived = new byte[1024];

            int byteUserRcvd = sender.Receive(userToSendReceived);
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
            int byteMessageSent = sender.Send(messageToSend);

            byte[] messageReceived = new byte[1024];
            int byteMessageRcvd = sender.Receive(messageReceived);
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
                login(username, password);
            }
            else if (choice == '2')
            {
                addUser();
            }
            else
            {
                return;
            }
        }

        private void defaultMessage(string command)
        {
            string jsonCommand = JsonConvert.SerializeObject(command);
            byte[] messageSent = Encoding.ASCII.GetBytes(jsonCommand);

            int byteSent = sender.Send(messageSent);

            byte[] messageReceived = new byte[1024];

            int byteRcvd = sender.Receive(messageReceived);
            string jsonString = Encoding.ASCII.GetString(messageReceived, 0, byteRcvd);
            string encodingString = JsonConvert.DeserializeObject(jsonString).ToString();
            Console.WriteLine(encodingString);
        }

        private void stop(string command)
        {
            string jsonCommand = JsonConvert.SerializeObject(command);
            byte[] messageSent = Encoding.ASCII.GetBytes(jsonCommand);

            sender.Send(messageSent);
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();

            continueListening = false;
        }

        public void login(string username, string password)
        {
            Console.WriteLine();
            string msg = "login";

            var userReq = usernameRequest(msg);


            if (userReq.ToLower().Contains("username")) enterUsername(username);


            var passRequested = passwordRequest();

            if (passRequested.ToLower().Contains("password"))
            {
                enterPassword(password);
            }
            else
            {
                Console.WriteLine(passRequested);
                return;
            }
            
            byte[] receiveLoginAnswer = new byte[1024];
            int loginAnswerReceived = sender.Receive(receiveLoginAnswer);
            string jsonLoginAnswer = Encoding.ASCII.GetString(receiveLoginAnswer, 0, loginAnswerReceived);
            string encodingLoginAnswer = JsonConvert.DeserializeObject(jsonLoginAnswer).ToString();

            if (encodingLoginAnswer == "loggedIn") 
            {
                Console.WriteLine($"\n{username} has logged in.");
                isLoggedIn = true;
            }
            else
            {
                Console.WriteLine(encodingLoginAnswer);
            }
        }

        private void logout(string command)
        {
            isLoggedIn = false;
            defaultMessage(command);
        }

        private void printUserInfo(string command)
        {
            string jsonCommand = JsonConvert.SerializeObject(command);
            byte[] messageSent = Encoding.ASCII.GetBytes(jsonCommand);
            int byteSent = sender.Send(messageSent);

            byte[] messageReceived = new byte[1024];

            int byteRcvd = sender.Receive(messageReceived);
            string jsonString = Encoding.ASCII.GetString(messageReceived, 0, byteRcvd);
            string encodingString = JsonConvert.DeserializeObject(jsonString).ToString();

            if (encodingString.ToLower().Equals("approved"))
            {
                Console.WriteLine("\nEnter username you'd like to check");
                string username = Console.ReadLine();
                defaultMessage(username);
            }
            else
            {
                defaultMessage(encodingString);
            }

            
        }

        private void addUser()
        {
            Console.WriteLine();
            string msg = "add";

            usernameRequest(msg);

            string username = Console.ReadLine();
            enterUsername(username);
            passwordRequest();

            string password = Console.ReadLine();
            enterPassword(password);
            byte[] messageReceivedPass = new byte[1024];
            int byteRcvdPass = sender.Receive(messageReceivedPass);
            string jsonStringpass = Encoding.ASCII.GetString(messageReceivedPass, 0, byteRcvdPass);
            string encodingStringpass = JsonConvert.DeserializeObject(jsonStringpass).ToString();
            Console.WriteLine(encodingStringpass);
        }

        private string usernameRequest(string command)
        {
            string jsonCommand = JsonConvert.SerializeObject(command);
            byte[] messageSentUsername = Encoding.ASCII.GetBytes(jsonCommand);
            sender.Send(messageSentUsername);
            byte[] messageReceivedUser = new byte[1024];
            int byteRcvdUser = sender.Receive(messageReceivedUser);
            string jsonString = Encoding.ASCII.GetString(messageReceivedUser, 0, byteRcvdUser);
            string encodingString = JsonConvert.DeserializeObject(jsonString).ToString();
            return encodingString;
        }

        private void enterUsername(string username)
        {
            string jsonSendUsername = JsonConvert.SerializeObject(username);
            byte[] sendUsername = Encoding.ASCII.GetBytes(jsonSendUsername);
            sender.Send(sendUsername);
        }

        private string passwordRequest()
        {
            byte[] receivePasswordRequest = new byte[1024];
            int passwordRequestReceived = sender.Receive(receivePasswordRequest);
            string jsonStringPasswordRequest = Encoding.ASCII.GetString(receivePasswordRequest, 0, passwordRequestReceived);
            string encodingStringPasswordRequest = JsonConvert.DeserializeObject(jsonStringPasswordRequest).ToString();
            //Console.WriteLine(encodingStringPasswordRequest);
            if (encodingStringPasswordRequest.ToLower().Equals("user doesn't exist."))
            {
                notLoggedInFlag = true;
            }

            return encodingStringPasswordRequest;
        }

        private void enterPassword(string password)
        {
            string jsonSendPassword = JsonConvert.SerializeObject(password);
            byte[] sendPassword = Encoding.ASCII.GetBytes(jsonSendPassword);
            sender.Send(sendPassword);
        }
    }
}