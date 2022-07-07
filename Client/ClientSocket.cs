using System;
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
                IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHost.AddressList[0];
                IPEndPoint localEndpoint = new IPEndPoint(ipAddress, 11111);

                bool flag = true;

                Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    sender.Connect(localEndpoint);
                    Console.WriteLine("Socket connected to -> {0}", sender.RemoteEndPoint.ToString());

                    while (true)
                    {


                        Console.Write("Enter a command: ");
                        string command = null;
                        command = Console.ReadLine();

                        byte[] messageSent = Encoding.ASCII.GetBytes(command);
                        int byteSent = sender.Send(messageSent);

                        byte[] messageReceived = new byte[1024];

                        int byteRcvd = sender.Receive(messageReceived);
                        //Console.WriteLine("Message from server -> {0}", Encoding.ASCII.GetString(messageReceived, 0, byteRcvd));

                        string encodingString = Encoding.ASCII.GetString(messageReceived, 0, byteRcvd);
                        string jasonCommand = JsonConvert.SerializeObject(encodingString);
                        
                        Console.WriteLine(jasonCommand);

                        if (jasonCommand == "stop") {

                            break;
                        }

                    } 
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
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