namespace Client.Interfaces;

public interface IUserService
{
    string AddUser(string username, string password);
    (bool, string) Login(string username, string password);
    string DeleteUser(string username);
    string GetUserInfo(string username);
    string GetCurrentRole();
    string GetLoggedInUser();
    void Logout();
    bool IsLoggedIn();
}