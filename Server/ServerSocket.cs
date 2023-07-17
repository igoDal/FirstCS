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
        private static string jsonMsg;
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

                    byte[] firstBytes = new byte[1024];
                    string jsonFirstData = null;

                    int firstNumByte = clientSocket.Receive(firstBytes);
                    jsonFirstData += Encoding.ASCII.GetString(firstBytes, 0, firstNumByte);
                    string firstData = JsonConvert.DeserializeObject(jsonFirstData).ToString();

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
                        jsonMsg = JsonConvert.SerializeObject($"Enter command (type \"help\" to check available commands): ");
                        message = Encoding.ASCII.GetBytes(jsonMsg);
                        clientSocket.Send(message);

                        byte[] bytes = new byte[1024];
                        string jsonData = null;

                        int numByte = clientSocket.Receive(bytes);
                        jsonData += Encoding.ASCII.GetString(bytes, 0, numByte);
                        string data = JsonConvert.DeserializeObject(jsonData).ToString();
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
            finally
            {
                listener.Close(); // Close the server socket listener
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
                    string jsonReadMsg = JsonConvert.SerializeObject(readMessage);
                    byte[] readMsgBytes = Encoding.ASCII.GetBytes(jsonReadMsg);
                    clientSocket.Send(readMsgBytes);
                    File.WriteAllLines(file, lines.Skip(1));
                }
                else
                {
                    jsonMsg = JsonConvert.SerializeObject($"There are no new messages.");
                    message = Encoding.ASCII.GetBytes(jsonMsg);
                    clientSocket.Send(message);
                }
            }
            else
            {
                jsonMsg = JsonConvert.SerializeObject($"There are no new messages.");
                message = Encoding.ASCII.GetBytes(jsonMsg);
                clientSocket.Send(message);
            }
        }

        private static void sendMessage()
        {
            jsonMsg = JsonConvert.SerializeObject($"Enter username:");
            message = Encoding.ASCII.GetBytes(jsonMsg);
            clientSocket.Send(message);

            string jsonUsername;
            int count = 0;

            int numByte = clientSocket.Receive(bytesU);
            jsonUsername = Encoding.ASCII.GetString(bytesU, 0, numByte);
            string username = JsonConvert.DeserializeObject(jsonUsername).ToString();
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

                string jsonMessage = JsonConvert.SerializeObject("Type your message: ");
                byte[] getMessage = Encoding.ASCII.GetBytes(jsonMessage);
                clientSocket.Send(getMessage);

                int numBytePassword = clientSocket.Receive(bytesP);
                string jsonPassMsg = Encoding.ASCII.GetString(bytesP, 0, numBytePassword);
                string passMsg = JsonConvert.DeserializeObject(jsonPassMsg).ToString();

                using (StreamReader sr = new StreamReader(msgFile))
                {
                    while (sr.ReadLine() != null)
                    {
                        count++;
                    }
                }

                if (count < 5)
                {
                    File.AppendAllText($"{username}_msg.txt", passMsg + "\n");

                    string jsonConfirmMsg = JsonConvert.SerializeObject("Message has been sent.");
                    byte[] confirmMsg = Encoding.ASCII.GetBytes(jsonConfirmMsg);
                    clientSocket.Send(confirmMsg);
                }
                else
                {
                    string jsonErrorMsg = JsonConvert.SerializeObject("Mailbox is full.");
                    byte[] errorMsg = Encoding.ASCII.GetBytes(jsonErrorMsg);
                    clientSocket.Send(errorMsg);
                }
            }
            else
            {
                string jsonConfMsg = JsonConvert.SerializeObject("User doesn't exist.");
                byte[] confirmMsg = Encoding.ASCII.GetBytes(jsonConfMsg);
                clientSocket.Send(confirmMsg);
            }
        }

        private static void printUserInfo()
        {

            if (currentRole.ToLower().Equals("admin"))
            {
                jsonMsg = JsonConvert.SerializeObject($"approved");
                message = Encoding.ASCII.GetBytes(jsonMsg);
                clientSocket.Send(message);

                string username;
                User singleUserData = null;
                
                int numByte = clientSocket.Receive(bytesU);
                string jsonUsername = Encoding.ASCII.GetString(bytesU, 0, numByte);
                username = JsonConvert.DeserializeObject(jsonUsername).ToString();

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
                    jsonMsg = JsonConvert.SerializeObject($"Username: {username}\n" +
                        $"Password: {getPassword}\n" +
                        $"Role: {getRole}\n");
                    byte[] msg = Encoding.ASCII.GetBytes(jsonMsg);
                    clientSocket.Send(msg);
                }
                else
                {
                    jsonMsg = JsonConvert.SerializeObject("User doesn't exist.");
                    byte[] msg = Encoding.ASCII.GetBytes(jsonMsg);
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

                jsonMsg = JsonConvert.SerializeObject($"Username: {loggedInUser}\n" +
                    $"Password: {getPassword}\n" +
                    $"Role: {currentRole}\n");
                byte[] msg = Encoding.ASCII.GetBytes(jsonMsg);
                clientSocket.Send(msg);
            }
        }

        private static void login()
        {
            jsonMsg = JsonConvert.SerializeObject($"Enter username:");
            message = Encoding.ASCII.GetBytes(jsonMsg);
            clientSocket.Send(message);

            string username;
            string password;
            int numByte = clientSocket.Receive(bytesU);
            string jsonUsername = Encoding.ASCII.GetString(bytesU, 0, numByte);
            username = JsonConvert.DeserializeObject(jsonUsername).ToString();
            var file = $"{username}.json";

            if (File.Exists(file))
            {
                var fileRead = File.ReadAllText(file);
                jsonMsg = JsonConvert.SerializeObject($"Enter password:");
                message = Encoding.ASCII.GetBytes(jsonMsg);
                clientSocket.Send(message);

                JsonReader line;
                int numBytePassword = clientSocket.Receive(bytesP);
                string jsonPassword = Encoding.ASCII.GetString(bytesP, 0, numBytePassword);
                password = JsonConvert.DeserializeObject(jsonPassword).ToString();

                var singleUserData = JsonConvert.DeserializeObject<User>(fileRead);
                string getPassword = singleUserData.Password;
                currentRole = singleUserData.Role;
                loggedInUser = singleUserData.Userame;
                


                Console.WriteLine(getPassword);
                if (getPassword.Equals(password))
                {
                    loggedIn = true;
                    jsonMsg = JsonConvert.SerializeObject($"loggedIn");
                    message = Encoding.ASCII.GetBytes(jsonMsg);
                    clientSocket.Send(message);
                }
                else
                {
                    jsonMsg = JsonConvert.SerializeObject($"Incorrect password!");
                    message = Encoding.ASCII.GetBytes(jsonMsg);
                    clientSocket.Send(message);
                }
            }
            else
            {
                jsonMsg = JsonConvert.SerializeObject($"user doesn't exist.");
                message = Encoding.ASCII.GetBytes(jsonMsg);
                clientSocket.Send(message);
            }
        }

            private static void logout()
        {
            loggedIn = false;

            jsonMsg = JsonConvert.SerializeObject("logout");
            byte[] message = Encoding.ASCII.GetBytes(jsonMsg);
            clientSocket.Send(message);
        }

        private static void addUser()
        {
            jsonMsg = JsonConvert.SerializeObject($"Enter username:");
            message = Encoding.ASCII.GetBytes(jsonMsg);
            clientSocket.Send(message);

            string username;
            string password;
            int numByte = clientSocket.Receive(bytesU);
            string jsonUsername = Encoding.ASCII.GetString(bytesU, 0, numByte);
            username = JsonConvert.DeserializeObject(jsonUsername).ToString();

            if (!File.Exists($"{username}.json"))
            {
                jsonMsg = JsonConvert.SerializeObject($"Enter password:");
                message = Encoding.ASCII.GetBytes(jsonMsg);
                clientSocket.Send(message);

                int numBytePassword = clientSocket.Receive(bytesP);
                string jsonPassword = Encoding.ASCII.GetString(bytesP, 0, numBytePassword);
                password = JsonConvert.DeserializeObject(jsonPassword).ToString();


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

                jsonMsg = JsonConvert.SerializeObject($"User {username} has been added.");
                message = Encoding.ASCII.GetBytes(jsonMsg);
            }

            else
            {
                jsonMsg = JsonConvert.SerializeObject($"User {username} already exists.");
                message = Encoding.ASCII.GetBytes(jsonMsg);
            }
            clientSocket.Send(message);
        }
        private static void deleteUser()
        {
            Console.WriteLine("Enter user (username) to delete: ");
            string username = Console.ReadLine();
            if (File.Exists($"{username}.txt"))
                File.Delete($"{username}.txt");

            jsonMsg = JsonConvert.SerializeObject($"User {username} has been removed.");
            byte[] message = Encoding.ASCII.GetBytes(jsonMsg);
            clientSocket.Send(message);
        }


        private static void incorrectCommand()
        {
            jsonMsg = JsonConvert.SerializeObject($"Incorrect command. Type 'help' to get list of commands.");
            byte[] message = Encoding.ASCII.GetBytes(jsonMsg);
            clientSocket.Send(message);
        }

        private static void stopCommand()
        {
            jsonMsg = JsonConvert.SerializeObject("stop");
            byte[] message = Encoding.ASCII.GetBytes(jsonMsg);
            clientSocket.Send(message);
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }

        private static void uptimeCommand()
        {
            DateTime serverCurrentDate = DateTime.Now;
            jsonMsg = JsonConvert.SerializeObject($"Server is up for {serverCurrentDate - serverCreationDate}");
            byte[] message = Encoding.ASCII.GetBytes(jsonMsg);
            clientSocket.Send(message);
        }

        private static void infoCommand()
        {
            jsonMsg = JsonConvert.SerializeObject($"Server version: {serverVersion}\n" +
                                $"Server Creation Date: {serverCreationDate}");
            byte[] message = Encoding.ASCII.GetBytes(jsonMsg);
            clientSocket.Send(message);
        }

        private static void helpCommand()
        {
            jsonMsg = JsonConvert.SerializeObject($"Available commands:\n" +
                                $"'add' - to add new user\n" +
                                $"'help' - to get a list of available commands with their description\n" +
                                $"'info' - to get info about server version, server creation date\n" +
                                $"'msg' - to send a message to other user\n" +
                                $"'read' - to read next message\n" +
                                $"'uptime' - to check server uptime\n" +
                                $"'user' - to print user data" +
                                $"'stop' - to stop the server\n" +
                                $"'logout' - to log out");
            byte[] message = Encoding.ASCII.GetBytes(jsonMsg);
            clientSocket.Send(message);
        }
    }
}