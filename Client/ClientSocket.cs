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
                    Console.WriteLine("Type '1' to login\n"+
                        "Type '2' to create new user\n" +
                        "Type anything else to quit");
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

                    while (isLoggedIn)
                    {
                        byte[] initialCommand = new byte[1024];

                        int initComm = sender.Receive(initialCommand);

                        string encodingInitComm = Encoding.ASCII.GetString(initialCommand, 0, initComm);
                        Console.WriteLine(encodingInitComm);

                        string command = Console.ReadLine();

                        byte[] messageSent = Encoding.ASCII.GetBytes(command);
                        int byteSent = sender.Send(messageSent);

                        byte[] messageReceived = new byte[1024];

                        int byteRcvd = sender.Receive(messageReceived);

                        string encodingString = Encoding.ASCII.GetString(messageReceived, 0, byteRcvd);
                        Console.WriteLine(encodingString);

                        if (encodingString == "Enter username:")
                        {
                            addUser();
                        }
                        if (encodingString == "logout")
                        {
                            isLoggedIn = false;
                            continue;
                        }

                        if (encodingString == "stop")
                        {
                            stop();
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
        }

        private static void enterPassword(string password)
        {
            byte[] sendPassword = Encoding.ASCII.GetBytes(password);
            sender.Send(sendPassword);
        }
    }
}