using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    class ServerSocket
    {
        static void Main(string[] args)
        {
            ExecuteServer();
        }

        public static void ExecuteServer()
        {
            string serverVersion = "0.0.1";
            DateTime serverCreationDate = DateTime.Now;

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

                    Socket clientSocket = listener.Accept();

                    Console.WriteLine("Connected");
                    while (true)
                    {
                        byte[] bytes = new byte[1024];
                        string data = null;

                        int numByte = clientSocket.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, numByte);
                        Console.WriteLine("Text received -> {0}", data);
                        byte[] message;
                        switch(data.ToLower())
                        {
                            case "help":
                                message = Encoding.ASCII.GetBytes($"Available commands:" +
                                    $"'info' - to get info about server version, server creation date" +
                                    $"'help' - to get a list of available commands with their description" +
                                    $"'uptime' - to check server uptime" +
                                    $"'stop' - to stop the server");
                                clientSocket.Send(message);
                                break;
                            case "info":
                                message = Encoding.ASCII.GetBytes($"Server version: {serverVersion}" +
                                    $"Server Creation Date: {serverCreationDate}");
                                clientSocket.Send(message);
                                break;
                            case "uptime":
                                DateTime serverCurrentDate = DateTime.Now;
                                message = Encoding.ASCII.GetBytes($"Server is up for {serverCurrentDate - serverCreationDate}");
                                clientSocket.Send(message);
                                break;
                            case "stop":
                                message = Encoding.ASCII.GetBytes("stop");
                                clientSocket.Send(message);
                                clientSocket.Shutdown(SocketShutdown.Both);
                                clientSocket.Close();

                                break;
                            default:
                                message = Encoding.ASCII.GetBytes($"Incorrect command. Type 'help' to get list of commands.");
                                clientSocket.Send(message);
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
    }
}