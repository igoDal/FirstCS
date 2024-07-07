using System.IO;
using Newtonsoft.Json;

namespace Server;
public class UserService
{
    private string currentRole;
    private string loggedInUser;
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
    public string GetCurrentRole()
    {
        return currentRole;
    }

    public string GetLoggedInUser()
    {
        return loggedInUser;
    }
}