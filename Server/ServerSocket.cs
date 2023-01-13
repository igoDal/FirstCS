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
        private static string currentRole;
        private static string loggedInUser;

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

                //Below lines were in while loop. Testing if it was correct.
                //--------------START----------------
                Console.WriteLine("Awaiting connection...");
                clientSocket = listener.Accept();
                Console.WriteLine("Connected");
                //---------------END-----------------


                while (true)
                {
                    //Checking if there should be one or two while loops. Probably first one (for !loggedIn) is not needed.
                    //--------------START----------------

                    //while (!loggedIn)
                    //{

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
                    //}
                    //---------------END-----------------

                    while (loggedIn)
                    {
                        message = Encoding.ASCII.GetBytes($"Enter command (type \"help\" to check available commands): ");
                        clientSocket.Send(message);

                        byte[] bytes = new byte[1024];
                        string data = null;

                        int numByte = clientSocket.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, numByte);
                        Console.WriteLine("Text received -> {0}", data);

                        switch (data.ToLower())
                        {
                            case "add":
                                addUser();
                                break;
                            case "user":
                                printUserInfo();
                                break;
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

                            case "msg":
                                sendMessage();
                                break;

                            case "read":
                                readMessage();
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

        private static void readMessage()
        {
            var file = $"{loggedInUser}_msg.txt";
            string readMessage = null;
            IEnumerable<string> lines = null;
            if (File.Exists(file))
            {
                using (StreamReader reader = File.OpenText(file))
                {
                    if (!reader.EndOfStream)
                    {
                        readMessage = reader.ReadLine();
                        lines = File.ReadAllLines(file);
                    }
                    else
                    {
                        readMessage = "none";
                    }
                }
                if (!readMessage.Equals("none"))
                {
                    byte[] readMsgBytes = Encoding.ASCII.GetBytes(readMessage);
                    clientSocket.Send(readMsgBytes);
                    File.WriteAllLines(file, lines.Skip(1));
                }
                else
                {
                    message = Encoding.ASCII.GetBytes($"There are no new messages.");
                    clientSocket.Send(message);
                }
            }
            else
            {
                message = Encoding.ASCII.GetBytes($"There are no new messages.");
                clientSocket.Send(message);
            }
        }

        private static void sendMessage()
        {
            message = Encoding.ASCII.GetBytes($"Enter username:");
            clientSocket.Send(message);

            string username;
            int count = 0;

            int numByte = clientSocket.Receive(bytesU);
            username = Encoding.ASCII.GetString(bytesU, 0, numByte);
            var file = $"{username}.json";
            var msgFile = $"{username}_msg.txt";
            if (File.Exists(file))
            {

                if (!File.Exists($"{username}_msg.txt"))
                {
                    using (StreamWriter sw = new StreamWriter(msgFile))
                    {
                    }
                }

                byte[] getMessage = Encoding.ASCII.GetBytes($"Type your message: ");
                clientSocket.Send(getMessage);

                int numBytePassword = clientSocket.Receive(bytesP);
                string message = Encoding.ASCII.GetString(bytesP, 0, numBytePassword);
                using(StreamReader sr = new StreamReader(msgFile))
                {
                    while (sr.ReadLine() != null)
                    {
                        count++;
                    }
                }

                if (count < 5)
                {
                    File.AppendAllText($"{username}_msg.txt", message + "\n");

                    byte[] confirmMsg = Encoding.ASCII.GetBytes("Message has been sent.");
                    clientSocket.Send(confirmMsg);
                }
                else
                {
                    byte[] errorMsg = Encoding.ASCII.GetBytes("Mailbox is full.");
                    clientSocket.Send(errorMsg);
                }
            }
            else
            {
                byte[] confirmMsg = Encoding.ASCII.GetBytes("User doesn't exist.");
                clientSocket.Send(confirmMsg);
            }
        }

        private static void printUserInfo()
        {

            if (currentRole.ToLower().Equals("admin"))
            {
                message = Encoding.ASCII.GetBytes($"approved");
                clientSocket.Send(message);

                string username;
                User singleUserData = null;
                
                int numByte = clientSocket.Receive(bytesU);
                username = Encoding.ASCII.GetString(bytesU, 0, numByte);
                var file = $"{username}.json";

                if (File.Exists(file))
                {

                    using (StreamReader reader = new StreamReader(file))
                    {
                        var fileRead = File.ReadAllText(file);
                        singleUserData = JsonConvert.DeserializeObject<User>(fileRead);
                    }
                    string getPassword = singleUserData.Password;
                    string getRole = singleUserData.Role;
                    loggedInUser = singleUserData.Userame;
                    byte[] msg = Encoding.ASCII.GetBytes($"Username: {username}\n" +
                        $"Password: {getPassword}\n" +
                        $"Role: {getRole}\n");
                    clientSocket.Send(msg);
                }
                else
                {
                    byte[] msg = Encoding.ASCII.GetBytes("User doesn't exist.");
                    clientSocket.Send(msg);
                }


            }
            else
            {
                var file = $"{loggedInUser}.json";
                User singleUserData = null;
                using (StreamReader reader = new StreamReader(file))
                {
                    var fileRead = File.ReadAllText(file);
                    singleUserData = JsonConvert.DeserializeObject<User>(fileRead);
                }
                
                string getPassword = singleUserData.Password;
                currentRole = singleUserData.Role;
                loggedInUser = singleUserData.Userame;
                byte[] msg = Encoding.ASCII.GetBytes($"Username: {loggedInUser}\n" +
                    $"Password: {getPassword}\n" +
                    $"Role: {currentRole}\n");
                clientSocket.Send(msg);
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
            var file = $"{username}.json";

            if (File.Exists(file))
            {
                var fileRead = File.ReadAllText(file);
                message = Encoding.ASCII.GetBytes($"Enter password:");
                clientSocket.Send(message);
                JsonReader line;
                int numBytePassword = clientSocket.Receive(bytesP);
                password = Encoding.ASCII.GetString(bytesP, 0, numBytePassword);

                var singleUserData = JsonConvert.DeserializeObject<User>(fileRead);
                string getPassword = singleUserData.Password;
                currentRole = singleUserData.Role;
                loggedInUser = singleUserData.Userame;
                


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

            private static void logout()
        {
            loggedIn = false;
            byte[] message = Encoding.ASCII.GetBytes("logout");
            clientSocket.Send(message);
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

                User user = new User()
                {
                    Userame = username,
                    Password = password,
                    Role = "user"
                };

                using (StreamWriter file = File.CreateText($"{username}.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, user);
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
                                $"'help' - to get a list of available commands with their description\n" +
                                $"'info' - to get info about server version, server creation date\n" +
                                $"'msg' - to send a message to other user\n" +
                                $"'read' - to read next message\n" +
                                $"'uptime' - to check server uptime\n" +
                                $"'user' - to print user data" +
                                $"'stop' - to stop the server\n" +
                                $"'logout' - to log out");
            clientSocket.Send(message);
        }
    }
}