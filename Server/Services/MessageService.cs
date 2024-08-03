using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Server.Interfaces;

namespace Server.Services
{
    public class MessageService : IMessageService
    {
        private ISocketWrapper _clientSocket;

        public void SetClientSocket(ISocketWrapper clientSocket)
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
                    SendData(readMessage);
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
        
        public void SendMessage()
        {
            SendData("Enter username:");
            string username = ReceiveData();
            var file = $"{username}.json";
            var msgFile = $"{username}_msg.txt";

            if (File.Exists(file))
            {
                if (!File.Exists(msgFile))
                {
                    using (StreamWriter sw = new StreamWriter(msgFile)) { }
                }

                SendData("Type your message:");
                string message = ReceiveData();

                int count = File.ReadAllLines(msgFile).Length;

                if (count < 5)
                {
                    File.AppendAllText(msgFile, message + "\n");
                    SendData("Message has been sent.");
                }
                else
                {
                    SendData("Mailbox is full.");
                }
            }
            else
            {
                SendData("User doesn't exist.");
            }
        }

        private void SendData(string message)
        {
            string jsonMsg = JsonConvert.SerializeObject(message);
            byte[] msg = Encoding.ASCII.GetBytes(jsonMsg);
            _clientSocket.Send(msg);
        }
        
        private string ReceiveData()
        {
            byte[] bytes = new byte[1024];
            int numByte = _clientSocket.Receive(bytes);
            string jsonString = Encoding.ASCII.GetString(bytes, 0, numByte);
            dynamic jsonResponse = JsonConvert.DeserializeObject(jsonString);
            return jsonResponse;
        }
    }
}