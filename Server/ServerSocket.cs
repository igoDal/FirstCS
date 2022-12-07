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

namespace Server
{
    class ServerSocket
    {
        private static Socket clientSocket;
        private readonly static string serverVersion = "0.0.3";
        private readonly static DateTime serverCreationDate = DateTime.Now;
        private static byte[] message;
        private static readonly byte[] bytes = new byte[1024];
        private static readonly byte[] bytesU = new byte[1024];
        private static readonly byte[] bytesP = new byte[1024];
        private static bool loggedIn = false;


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

                    byte[] firstBytes = new byte[1024];
                    string firstData = null;

                    int firstNumByte = clientSocket.Receive(firstBytes);
                    firstData += Encoding.ASCII.GetString(firstBytes, 0, firstNumByte);

                    if (firstData.ToLower() == "login")
                    {
                        login();
                    }
                    else if (firstData.ToLower() == "add")
                    {
                        addUser();
                    }
                    else
                        break;

                    while (loggedIn)
                    {
                        message = Encoding.ASCII.GetBytes($"Enter command:");
                        clientSocket.Send(message);

                        byte[] bytes = new byte[1024];
                        string data = null;

                        int numByte = clientSocket.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, numByte);
                        Console.WriteLine("Text received -> {0}", data);

                        switch (data.ToLower())
                        {
                            case "help":
                                helpCommand();
                                break;

                            case "info":
                                infoCommand();
                                break;
                            
                            case "uptime":
                                uptimeCommand();
                                break;
                            
                            case "stop":
                                stopCommand();
                                break;

                            case "logout":
                                logout();
                                break;

                            case "delete":
                                deleteUser();
                                break;

                            default:
                                incorrectCommand(); 
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

        private static void login()
        {
            message = Encoding.ASCII.GetBytes($"Enter username:");
            clientSocket.Send(message);

            string username;
            string password;
            int numByte = clientSocket.Receive(bytesU);
            username = Encoding.ASCII.GetString(bytesU, 0, numByte);

            if (File.Exists($"{username}.json"))
            {
                message = Encoding.ASCII.GetBytes($"Enter password:");
                clientSocket.Send(message);
                JsonReader line;
                int numBytePassword = clientSocket.Receive(bytesP);
                password = Encoding.ASCII.GetString(bytesP, 0, numBytePassword);
                
                var json = $"{username}.json";
                var data = (JObject)JsonConvert.DeserializeObject(json);
                string getPassword = data["Password"].Value<string>();

                Console.WriteLine(getPassword);
                if (getPassword.Equals(password))
                {
                    loggedIn = true;
                    message = Encoding.ASCII.GetBytes($"loggedIn");
                    clientSocket.Send(message);
                }
                else
                {
                    message = Encoding.ASCII.GetBytes($"Incorrect password!");
                    clientSocket.Send(message);
                }
            }
            else
            {
                message = Encoding.ASCII.GetBytes($"user doesn't exist.");
                clientSocket.Send(message);
            }
        }

        private static void addUser()
        {
            message = Encoding.ASCII.GetBytes($"Enter username:");
            clientSocket.Send(message);

            string username;
            string password;
            int numByte = clientSocket.Receive(bytesU);
            username = Encoding.ASCII.GetString(bytesU, 0, numByte);

            if (!File.Exists($"{username}.json"))
            {
                message = Encoding.ASCII.GetBytes($"Enter password:");
                clientSocket.Send(message);

                int numBytePassword = clientSocket.Receive(bytesP);
                password = Encoding.ASCII.GetString(bytesP, 0, numBytePassword);

                List<User> _user = new List<User>();
                _user.Add(new User()
                {
                    Userame = username,
                    Password = password,
                    Role = "user"
                });

                using (StreamWriter file = File.CreateText($"{username}.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, _user);
                }
                message = Encoding.ASCII.GetBytes($"User {username} has been added.");
            }

            else
            {
                message = Encoding.ASCII.GetBytes($"User {username} already exists.");
            }

            clientSocket.Send(message);
        }
        private static void deleteUser()
        {
            Console.WriteLine("Enter user (username) to delete: ");
            string username = Console.ReadLine();
            if (File.Exists($"{username}.txt"))
                File.Delete($"{username}.txt");
            byte[] message = Encoding.ASCII.GetBytes($"User {username} has been removed.");
            clientSocket.Send(message);

        }

            private static void logout()
        {
            byte[] message = Encoding.ASCII.GetBytes("logout");
            clientSocket.Send(message);
        }

        private static void incorrectCommand()
        {
            byte[] message = Encoding.ASCII.GetBytes($"Incorrect command. Type 'help' to get list of commands.");
            clientSocket.Send(message);
        }

        private static void stopCommand()
        {
            byte[] message = Encoding.ASCII.GetBytes("stop");
            clientSocket.Send(message);
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }

        private static void uptimeCommand()
        {
            DateTime serverCurrentDate = DateTime.Now;
            byte[] message = Encoding.ASCII.GetBytes($"Server is up for {serverCurrentDate - serverCreationDate}");
            clientSocket.Send(message);
        }

        private static void infoCommand()
        {
            byte[] message = Encoding.ASCII.GetBytes($"Server version: {serverVersion}\n" +
                                $"Server Creation Date: {serverCreationDate}");
            clientSocket.Send(message);
        }

        private static void helpCommand()
        {
            byte[] message = Encoding.ASCII.GetBytes($"Available commands:\n" +
                                $"'add' - to add new user\n" +
                                $"'info' - to get info about server version, server creation date\n" +
                                $"'help' - to get a list of available commands with their description\n" +
                                $"'uptime' - to check server uptime\n" +
                                $"'stop' - to stop the server\n");
            clientSocket.Send(message);
        }
    }
}