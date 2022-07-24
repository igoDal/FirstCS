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
                //For now don't know how to move it to config file.
                IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHost.AddressList[0];
                IPEndPoint localEndpoint = new IPEndPoint(ipAddress, 11111);

                sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    sender.Connect(localEndpoint);
                    Console.WriteLine("Socket connected to -> {0}", sender.RemoteEndPoint.ToString());
                    //login();

                    while (isLoggedIn)
                    {

                        Console.Write("Enter a command: ");
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

        //private static void login()
        //{
        //    Console.WriteLine("Podaj login: ");
        //    string username = Console.ReadLine();

        //    Console.WriteLine("Podaj hasło: ");
        //    string password = Console.ReadLine();

        //    if (File.Exists($"{username}.json"))
        //    {
        //        isLoggedIn = true;
        //    }
        //    else
        //    {
                
        //        bool loopFlag = true;
        //        while (loopFlag)
        //        {
        //            Console.WriteLine("User doesn't exist. Would you like to create this user?\n" +
        //            "Type '1' to create new user\n" +
        //            "Type '2' to abort");
        //            char choice = Console.ReadKey().KeyChar;

        //            switch (choice)
        //            {
        //                case '1':
        //                    addUser();
        //                    break;
        //                    loopFlag = false;
        //                case '2':
        //                    stop();
        //                    loopFlag = false;
        //                    break;
        //                default:
        //                    Console.WriteLine("Incorrect choice, try again.");
        //                    break;
        //            }
        //            if (choice == '1' || choice == '2')
        //                break;
        //        }

        //    }
        //}

        private static void addUser()
        {
            while (true)
            {

                string username = Console.ReadLine();

                byte[] messageSentUsername = Encoding.ASCII.GetBytes(username);
                int byteSentUser = sender.Send(messageSentUsername);

                byte[] messageReceivedUser = new byte[1024];

                int byteRcvdUser = sender.Receive(messageReceivedUser);

                string encodingString = Encoding.ASCII.GetString(messageReceivedUser, 0, byteRcvdUser);
                Console.WriteLine(encodingString);

                string password = Console.ReadLine();

                byte[] messageSentPass = Encoding.ASCII.GetBytes(username);
                int byteSentPass = sender.Send(messageSentUsername);

                byte[] messageReceivedPass = new byte[1024];

                int byteRcvdPass = sender.Receive(messageReceivedPass);

                string encodingStringpass = Encoding.ASCII.GetString(messageReceivedPass, 0, byteRcvdPass);
                Console.WriteLine(encodingStringpass);

                break;
            }
        }
    }
}