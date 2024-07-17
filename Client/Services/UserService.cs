using System.IO;
using Client.Interfaces;
using Newtonsoft.Json;

namespace Client.Services;

public class UserService : IUserService
    {
        private string currentRole;
        private string loggedInUser;
        private bool isLoggedIn;

        public string AddUser(string username, string password)
        {
            if (File.Exists($"{username}.json"))
            {
                return $"User {username} already exists.";
            }

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

            return $"User {username} has been added.";
        }

        public (bool, string) Login(string username, string password)
        {
            var file = $"{username}.json";
            if (File.Exists(file))
            {
                var fileRead = File.ReadAllText(file);
                var singleUserData = JsonConvert.DeserializeObject<User>(fileRead);
                string getPassword = singleUserData.Password;
                currentRole = singleUserData.Role;
                loggedInUser = singleUserData.Userame;

                if (getPassword.Equals(password))
                {
                    isLoggedIn = true;
                    return (true, "loggedIn");
                }
                else
                {
                    return (false, "Incorrect password!");
                }
            }
            else
            {
                return (false, "User doesn't exist.");
            }
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

        public bool IsLoggedIn()
        {
            return isLoggedIn;
        }
    }