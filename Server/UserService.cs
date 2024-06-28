using System.IO;
using Newtonsoft.Json;

namespace Server;
public class UserService
{
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
}