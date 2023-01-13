using System.Configuration;
using System.Collections.Specialized;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace Client
{
    class ClientSocket
    {
        private static bool isLoggedIn = false;
        private static bool isOnline = false;
        private static Socket sender;
        private static bool notLoggedInFlag = false;
        static void Main(string[] args)
        {
            ExecuteClient();
        }

        static void ExecuteClient()
        {
            try
            {
                //For now I don't know how to move it to config file.
                IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHost.AddressList[0];
                IPEndPoint localEndpoint = new IPEndPoint(ipAddress, 11111);

                sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    sender.Connect(localEndpoint);
                    Console.WriteLine("Socket connected to -> {0}", sender.RemoteEndPoint.ToString());

                    while (true)
                    {
                        Menu();

                        while (isLoggedIn)
                        {
                            // Whole method needs to be changed

                            //------------START-------------
                            
                            byte[] initialCommand = new byte[1024];

                            int initComm = sender.Receive(initialCommand);

                            string encodingInitComm = Encoding.ASCII.GetString(initialCommand, 0, initComm);
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
                                    stop();
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
        }

        private static void readMessage(string command)
        {
            defaultMessage(command);
        }

        private static void sendMessage(string command)
        {
            //Call sendMessage method on server side
            byte[] msgCommand = Encoding.ASCII.GetBytes(command);
            int byteSent = sender.Send(msgCommand);
            byte[] msgReceived = new byte[1024];
            int byteRcvd = sender.Receive(msgReceived);
            string encodingString = Encoding.ASCII.GetString(msgReceived, 0, byteRcvd);
            Console.WriteLine(encodingString);
            
            //Request for username (message receiver)
            //Console.WriteLine("Type a message: ");
            string userToSend = Console.ReadLine();
            byte[] usernameSent = Encoding.ASCII.GetBytes(userToSend);
            int byteUserToSend = sender.Send(usernameSent);

            byte[] userToSendReceived = new byte[1024];

            int byteUserRcvd = sender.Receive(userToSendReceived);

            string encodingUserString = Encoding.ASCII.GetString(userToSendReceived, 0, byteUserRcvd);
            Console.WriteLine(encodingUserString);

            //Console.WriteLine("Enter a message: ");
            string message = Console.ReadLine();
            const int MAX_LENGTH = 255;
            if (message.Length > MAX_LENGTH)
            {
                message = message.Substring(0, MAX_LENGTH);
            }
            byte[] messageToSend = Encoding.ASCII.GetBytes(message);
            int byteMessageSent = sender.Send(messageToSend);

            byte[] messageReceived = new byte[1024];
            int byteMessageRcvd = sender.Receive(messageReceived);
            string encodingStringMessage = Encoding.ASCII.GetString(messageReceived, 0, byteMessageRcvd);
            Console.WriteLine(encodingStringMessage);
        }

        private static void Menu()
        {
            Console.WriteLine("Type '1' to login\n" +
                            "Type '2' to create new user\n"); //+
                            //"Type other number to quit");
            char choice = Console.ReadKey().KeyChar;
            if (choice == '1')
            {
                login();
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

        private static void defaultMessage(string command)
        {
            byte[] messageSent = Encoding.ASCII.GetBytes(command);
            int byteSent = sender.Send(messageSent);

            byte[] messageReceived = new byte[1024];

            int byteRcvd = sender.Receive(messageReceived);

            string encodingString = Encoding.ASCII.GetString(messageReceived, 0, byteRcvd);
            Console.WriteLine(encodingString);
        }

        private static void stop()
        {
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        private static void login()
        {
            Console.WriteLine();
            string msg = "login";

            usernameRequest(msg);

            string username = Console.ReadLine();

            enterUsername(username);
            
            passwordRequest();

            //Needed to add "notLoggedIn" flag to call out to break method here if user doesn't exist.
            if (notLoggedInFlag == true)
            {
                notLoggedInFlag = false;
                return;
            }

            string password = Console.ReadLine();
            enterPassword(password);
            byte[] receiveLoginAnswer = new byte[1024];
            int loginAnswerReceived = sender.Receive(receiveLoginAnswer);

            string encodingLoginAnswer = Encoding.ASCII.GetString(receiveLoginAnswer, 0, loginAnswerReceived);

            if (encodingLoginAnswer == "loggedIn") 
            {
                Console.WriteLine($"{username} has logged in.");
                isLoggedIn = true;
            }
            else
            {
                Console.WriteLine(encodingLoginAnswer);

            }
        }

        private static void logout(string command)
        {
            isLoggedIn = false;
            defaultMessage(command);
        }

        private static void printUserInfo(string command)
        {
            byte[] messageSent = Encoding.ASCII.GetBytes(command);
            int byteSent = sender.Send(messageSent);

            byte[] messageReceived = new byte[1024];

            int byteRcvd = sender.Receive(messageReceived);

            string encodingString = Encoding.ASCII.GetString(messageReceived, 0, byteRcvd);

            if (encodingString.ToLower().Equals("approved"))
            {
                Console.WriteLine("Enter username you'd like to check");
                string username = Console.ReadLine();
                //enterUsername(username);
                defaultMessage(username);
            }
            else
            {
                //Console.WriteLine(encodingString);
                defaultMessage(encodingString);
            }

            
        }

        private static void addUser()
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
            string encodingStringpass = Encoding.ASCII.GetString(messageReceivedPass, 0, byteRcvdPass);
            Console.WriteLine(encodingStringpass);
        }

        private static void usernameRequest(string command)
        {
            byte[] messageSentUsername = Encoding.ASCII.GetBytes(command);
            sender.Send(messageSentUsername);
            byte[] messageReceivedUser = new byte[1024];
            int byteRcvdUser = sender.Receive(messageReceivedUser);
            string encodingString = Encoding.ASCII.GetString(messageReceivedUser, 0, byteRcvdUser);
            Console.WriteLine(encodingString);
        }

        private static void enterUsername(string username)
        {
            byte[] sendUsername = Encoding.ASCII.GetBytes(username);
            sender.Send(sendUsername);
        }

        private static void passwordRequest()
        {
            byte[] receivePasswordRequest = new byte[1024];
            int passwordRequestReceived = sender.Receive(receivePasswordRequest);
            string encodingStringPasswordRequest = Encoding.ASCII.GetString(receivePasswordRequest, 0, passwordRequestReceived);
            Console.WriteLine(encodingStringPasswordRequest);
            if (encodingStringPasswordRequest.ToLower().Equals("user doesn't exist."))
            {
                notLoggedInFlag = true;
            }
        }

        private static void enterPassword(string password)
        {
            byte[] sendPassword = Encoding.ASCII.GetBytes(password);
            sender.Send(sendPassword);
        }
    }
}