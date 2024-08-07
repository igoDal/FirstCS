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
}
