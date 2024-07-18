using System;
using System.Text;
using Client.Interfaces;
using Newtonsoft.Json;

namespace Client.Services;

public class MessageService : IMessageService
    {
        public void SendMessage(string command, ISocketWrapper socketWrapper)
        {
            string jsonCommand = JsonConvert.SerializeObject(command);
            byte[] msgCommand = Encoding.ASCII.GetBytes(jsonCommand);
            socketWrapper.Send(msgCommand);

            byte[] msgReceived = new byte[1024];
            int byteRcvd = socketWrapper.Receive(msgReceived);
            string jsonString = Encoding.ASCII.GetString(msgReceived, 0, byteRcvd);
            string encodingString;
            try
            {
                var deserializedObject = JsonConvert.DeserializeObject(jsonString);
                encodingString = deserializedObject.ToString();
            }
            catch (JsonReaderException)
            {
                encodingString = jsonString;
            }

            Console.WriteLine(encodingString);

            string userToSend = Console.ReadLine();
            string jsonUserToSend = JsonConvert.SerializeObject(userToSend);
            byte[] usernameSent = Encoding.ASCII.GetBytes(jsonUserToSend);
            socketWrapper.Send(usernameSent);

            byte[] userToSendReceived = new byte[1024];
            int byteUserRcvd = socketWrapper.Receive(userToSendReceived);
            string jsonUserString = Encoding.ASCII.GetString(userToSendReceived, 0, byteUserRcvd);
            string encodingUserString = JsonConvert.DeserializeObject(jsonUserString).ToString();
            Console.WriteLine(encodingUserString);

            string message = Console.ReadLine();
            const int MAX_LENGTH = 255;
            if (message.Length > MAX_LENGTH)
            {
                message = message.Substring(0, MAX_LENGTH);
            }
            string jsonMessage = JsonConvert.SerializeObject(message);
            byte[] messageToSend = Encoding.ASCII.GetBytes(jsonMessage);
            socketWrapper.Send(messageToSend);

            byte[] messageReceived = new byte[1024];
            int byteMessageRcvd = socketWrapper.Receive(messageReceived);
            string jsonStringMessage = Encoding.ASCII.GetString(messageReceived, 0, byteMessageRcvd);
            string encodingStringMessage = JsonConvert.DeserializeObject(jsonStringMessage).ToString();
            Console.WriteLine(encodingStringMessage);
        }

        public void ReadMessage(string command, ISocketWrapper socketWrapper)
        {
            string jsonCommand = JsonConvert.SerializeObject(command);
            byte[] messageSent = Encoding.ASCII.GetBytes(jsonCommand);
            socketWrapper.Send(messageSent);

            byte[] messageReceived = new byte[1024];
            int byteRcvd = socketWrapper.Receive(messageReceived);
            string jsonString = Encoding.ASCII.GetString(messageReceived, 0, byteRcvd);
            string encodingString = JsonConvert.DeserializeObject(jsonString).ToString();
            Console.WriteLine(encodingString);
        }
    }