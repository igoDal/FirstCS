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

                Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                bool isLoggedIn = false;

                try
                {
                    sender.Connect(localEndpoint);
                    Console.WriteLine("Socket connected to -> {0}", sender.RemoteEndPoint.ToString());

                    Console.WriteLine("Podaj login: ");
                    string username = Console.ReadLine();
                    Console.WriteLine("Podaj hasło: ");
                    string password = Console.ReadLine();

                    Console.WriteLine($"Hello {username}");

                    isLoggedIn = true;

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

                        if(encodingString == "logout")
                        {
                            isLoggedIn = false;
                            continue;
                        }

                        if (encodingString == "stop")
                        {
                            sender.Shutdown(SocketShutdown.Both);
                            sender.Close();
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
    }
}