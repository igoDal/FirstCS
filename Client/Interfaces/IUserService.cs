namespace Client.Interfaces;

public interface IUserService
{
    bool IsLoggedIn { get; }
    string AddUser();
    bool Login();
    string DeleteUser(string username);
    string GetUserInfo(string username);
    string GetCurrentRole();
    string GetLoggedInUser();
    void Logout();
    void PrintUserInfo(string command);
}