using System.IO;
using Newtonsoft.Json;
using Server.Services;
using Xunit;

namespace Server.Tests;
public class UserServiceTests
{
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userService = new UserService();
    }
    
    [Fact]
    public void AddUser_ShouldAddNewUser()
    {
        // Arrange
        var username = "123aaa";
        var password = "password123";
        var expectedFilePath = $"{username}.json";

        // Act
        var result = _userService.AddUser(username, password);

        // Assert
        Assert.Equal($"User {username} has been added.", result);
        Assert.True(File.Exists(expectedFilePath));
            
        if (File.Exists(expectedFilePath))
        {
            File.Delete(expectedFilePath);
        }
    }

    [Fact]
    public void AddUser_ShouldNotAddExistingUser()
    {
        // Arrange
        var username = "existinguser";
        var password = "password123";
        var expectedFilePath = $"{username}.json";
            
        _userService.AddUser(username, password);

        // Act
        var result = _userService.AddUser(username, password);

        // Assert
        Assert.Equal($"User {username} already exists.", result);
            
        if (File.Exists(expectedFilePath))
        {
            File.Delete(expectedFilePath);
        }
    }

    [Fact]
    public void Login_ShouldReturnTrueAndLoggedIn_WhenCredentialsAreCorrect()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";
        var user = new User
        {
            Userame = username,
            Password = password,
            Role = "user"
        };
        File.WriteAllText($"{username}.json", JsonConvert.SerializeObject(user));

        // Act
        var result = _userService.Login(username, password);

        // Assert
        Assert.True(result.Item1);
        Assert.Equal("loggedIn", result.Item2);
        Assert.True(_userService.IsLoggedIn());
        Assert.Equal(username, _userService.GetLoggedInUser());
        Assert.Equal("user", _userService.GetCurrentRole());

        File.Delete($"{username}.json");
    }

    [Fact]
    public void Login_ShouldReturnFalseAndIncorrectPassword_WhenPasswordIsIncorrect()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";
        var incorrectPassword = "wrongpassword";
        var user = new User
        {
            Userame = username,
            Password = password,
            Role = "user"
        };
        File.WriteAllText($"{username}.json", JsonConvert.SerializeObject(user));

        // Act
        var result = _userService.Login(username, incorrectPassword);

        // Assert
        Assert.False(result.Item1);
        Assert.Equal("Incorrect password!", result.Item2);
        Assert.False(_userService.IsLoggedIn());
        Assert.Null(_userService.GetLoggedInUser());

        File.Delete($"{username}.json");
    }

    [Fact]
    public void Login_ShouldReturnFalseAndUserDoesNotExist_WhenUserDoesNotExist()
    {
        // Arrange
        var username = "nonexistentuser";
        var password = "password123";

        // Act
        var result = _userService.Login(username, password);

        // Assert
        Assert.False(result.Item1);
        Assert.Equal("User doesn't exist.", result.Item2);
        Assert.False(_userService.IsLoggedIn());
        Assert.Null(_userService.GetLoggedInUser());
    }
    
    [Fact]
    public void GetUserInfo_ShouldReturnCorrectUserInfo_WhenUserExists()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";
        var role = "user";
        var expectedUserInfo = $"Username: {username}\nPassword: {password}\nRole: {role}";
        
        var user = new User
        {
            Userame = username,
            Password = password,
            Role = role
        };
        File.WriteAllText($"{username}.json", JsonConvert.SerializeObject(user));

        // Act
        var result = _userService.GetUserInfo(username);

        // Assert
        Assert.Equal(expectedUserInfo, result);
        
        File.Delete($"{username}.json");
    }

    [Fact]
    public void GetUserInfo_ShouldReturnErrorMessage_WhenUserDoesNotExist()
    {
        // Arrange
        var username = "nonexistentuser";
        var expectedMessage = "User file not found.";

        // Act
        var result = _userService.GetUserInfo(username);

        // Assert
        Assert.Equal(expectedMessage, result);
    }
}
