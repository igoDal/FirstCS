﻿using System;
using System.IO;
using System.Text;
using Client.Interfaces;
using Newtonsoft.Json;

namespace Client.Services;

public class UserService(ISocketWrapper _socketWrapper) : IUserService
    {
        private string currentRole;
        private string loggedInUser;
        private bool isLoggedIn;
        public bool IsLoggedIn => isLoggedIn;

        public string AddUser()
        {
            string usernamePrompt = ReceiveJsonData();
            Console.WriteLine(usernamePrompt);

            string username = Console.ReadLine();
            SendData(username);

            string passwordPrompt = ReceiveJsonData();
            Console.WriteLine(passwordPrompt);

            string password = Console.ReadLine();
            SendData(password);

            string result = ReceiveJsonData();
            return result;
        }
        
        public bool Login()
        {
            string usernamePrompt = ReceiveJsonData();
            Console.WriteLine(usernamePrompt);

            string username = Console.ReadLine();
            SendData(username);

            string passwordPrompt = ReceiveJsonData();
            Console.WriteLine(passwordPrompt);

            string password = Console.ReadLine();
            SendData(password);

            string response = ReceiveJsonData();

            if (response.Equals("loggedIn", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("\nLogin successful. Awaiting further commands.");
                isLoggedIn = true;
                loggedInUser = username;
                currentRole = "user"; 
                return true;
            }

            return false;
        }
        
        public string DeleteUser(string username)
        {
            var file = $"{username}.json";
            if (File.Exists(file))
            {
                File.Delete(file);
                var msgFile = $"{username}_msg.txt";
                if (File.Exists(msgFile))
                {
                    File.Delete(msgFile);
                }
                return $"User {username} has been deleted.";
            }
            else
            {
                return $"User {username} does not exist.";
            }
        }

        public string GetUserInfo(string username)
        {
            var file = $"{username}.json";
            if (File.Exists(file))
            {
                var fileRead = File.ReadAllText(file);
                var singleUserData = JsonConvert.DeserializeObject<User>(fileRead);

                return $"Username: {singleUserData.Userame}\nPassword: {singleUserData.Password}\nRole: {singleUserData.Role}";
            }
            else
            {
                return "User file not found.";
            }
        }

        public string GetCurrentRole()
        {
            return currentRole;
        }

        public string GetLoggedInUser()
        {
            return loggedInUser;
        }

        public void Logout()
        {
            isLoggedIn = false;
            loggedInUser = null;
            currentRole = null;
        }
        
        public void PrintUserInfo(string command)
        {
            SendData(command);

            string encodingString = ReceiveJsonData();

            if (encodingString.ToLower().Equals("approved"))
            {
                Console.WriteLine("\nEnter username you'd like to check");
                string username = Console.ReadLine();
                DefaultMessage(username);
            }
            else
            {
                var currentUser = GetLoggedInUser();
                DefaultMessage(currentUser);
            }
        }
        
        private void DefaultMessage(string command)
        {
            SendData(command);

            string jsonResponse = ReceiveJsonData();
            Console.WriteLine(jsonResponse);
        }
        
        private void SendData(string data)
        {
            string jsonData = JsonConvert.SerializeObject(new { command = data });
            byte[] messageSent = Encoding.ASCII.GetBytes(jsonData);
            _socketWrapper.Send(messageSent);
        }
        
        private string ReceiveJsonData()
        {
            byte[] bytes = new byte[1024];
            int numByte = _socketWrapper.Receive(bytes);
            string jsonString = Encoding.ASCII.GetString(bytes, 0, numByte);
            dynamic jsonResponse = JsonConvert.DeserializeObject(jsonString);
            return jsonResponse.command;
        }
    }