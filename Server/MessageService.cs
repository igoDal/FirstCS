using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    public class MessageService
    {
        private readonly Socket _clientSocket;

        public MessageService(Socket clientSocket)
        {
            _clientSocket = clientSocket;
        }

        public void ReadMessage(string loggedInUser)
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
                    _clientSocket.Send(readMsgBytes);
                    File.WriteAllLines(file, lines.Skip(1));
                }
                else
                {
                    SendData("There are no new messages.");
                }
            }
            else
            {
                SendData("There are no new messages.");
            }
        }

        private void SendData(string message)
        {
            string jsonMsg = JsonConvert.SerializeObject(message);
            byte[] msg = Encoding.ASCII.GetBytes(jsonMsg);
            _clientSocket.Send(msg);
        }
    }
}