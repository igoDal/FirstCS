namespace Client.Interfaces;

public interface IUserService
{
    bool IsLoggedIn { get; }
    string AddUser(string username, string password);
    void Login();
    string DeleteUser(string username);
    string GetUserInfo(string username);
    string GetCurrentRole();
    string GetLoggedInUser();
    void Logout();
}