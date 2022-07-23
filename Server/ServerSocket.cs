using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Configuration;
using System.Collections.Specialized;
using System.IO;

namespace Server
{
    class ServerSocket
    {
        private static Socket clientSocket;
        private readonly static string serverVersion = "0.0.3";
        private readonly static DateTime serverCreationDate = DateTime.Now;


        static void Main(string[] args)
        {
            ExecuteServer();
        }

        public static void ExecuteServer()
        {
            
            

            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHost.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11111);

            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                while (true)
                {
                    Console.WriteLine("Awaiting connection...");

                    clientSocket = listener.Accept();

                    Console.WriteLine("Connected");

                    while (true)
                    {
                        byte[] bytes = new byte[1024];
                        string data = null;

                        int numByte = clientSocket.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, numByte);
                        Console.WriteLine("Text received -> {0}", data);
                        byte[] message;

                        switch (data.ToLower())
                        {
                            case "help":
                                HelpCommand(data);
                                break;

                            case "info":
                                infoCommand(data);
                                break;
                            
                            case "uptime":
                                uptimeCommand(data);
                                break;
                            
                            case "stop":
                                stopCommand(data);
                                break;

                            case "logout":
                                logout(data);
                                break;

                            case "add":
                                addUser(data);
                                break;

                            case "delete":
                                deleteUser(data);
                                break;

                            default:
                                incorrectCommand(data); 
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        private static void deleteUser(string data)
        {
            Console.WriteLine("Enter user (username) to delete: ");
            string username = Console.ReadLine();
            if (File.Exists($"{username}.json"))
                File.Delete($"{username}.json");
            byte[] message = Encoding.ASCII.GetBytes($"User {username} has been removed.");
            clientSocket.Send(message);

        }

        private static void addUser(string data)
        {
            Console.WriteLine("Enter user (username) to add:");
            string username = Console.ReadLine();
            Console.WriteLine("Enter password:");
            string password = Console.ReadLine();
            
            using (var streamWriter = new StreamWriter($"{username}.json"))
            {
                streamWriter.WriteLine(username);
                streamWriter.WriteLine(password);
            }

            byte[] message = Encoding.ASCII.GetBytes($"User {username} has been added.");
            clientSocket.Send(message);
        }

        private static void logout(string data)
        {
            byte[] message = Encoding.ASCII.GetBytes("logout");
            clientSocket.Send(message);
        }

        private static void incorrectCommand(string data)
        {
            byte[] message = Encoding.ASCII.GetBytes($"Incorrect command. Type 'help' to get list of commands.");
            clientSocket.Send(message);
        }

        private static void stopCommand(string data)
        {
            byte[] message = Encoding.ASCII.GetBytes("stop");
            clientSocket.Send(message);
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }

        private static void uptimeCommand(string data)
        {
            DateTime serverCurrentDate = DateTime.Now;
            byte[] message = Encoding.ASCII.GetBytes($"Server is up for {serverCurrentDate - serverCreationDate}");
            clientSocket.Send(message);
        }

        private static void infoCommand(string command)
        {
            byte[] message = Encoding.ASCII.GetBytes($"Server version: {serverVersion}\n" +
                                $"Server Creation Date: {serverCreationDate}");
            clientSocket.Send(message);
        }

        public static void HelpCommand(string command)
        {
            byte[] message = Encoding.ASCII.GetBytes($"Available commands:\n" +
                                $"'info' - to get info about server version, server creation date\n" +
                                $"'help' - to get a list of available commands with their description\n" +
                                $"'uptime' - to check server uptime\n" +
                                $"'stop' - to stop the server\n");
            clientSocket.Send(message);
        }
    }
}