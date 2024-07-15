namespace Server.Interfaces;

public interface IUserService
{
    string AddUser(string username, string password);
    (bool, string) Login(string username, string password);
    string GetCurrentRole();
    string GetLoggedInUser();
    void Logout();
    bool IsLoggedIn();
    string DeleteUser(string username);
}