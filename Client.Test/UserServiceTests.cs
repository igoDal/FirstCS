using System.Text;
using Client.Interfaces;
using Client.Services;
using Moq;
using Newtonsoft.Json;

namespace Client.Test;

public class UserServiceTests
{
    private readonly Mock<ISocketWrapper> _mockSocketWrapper;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockSocketWrapper = new Mock<ISocketWrapper>();
        _userService = new UserService(_mockSocketWrapper.Object);
    }
    
    [Fact]
    public void DeleteUser_ShouldSendSuccessMessage_WhenUserIsDeletedSuccessfully()
    {
        // Arrange
        var username = "userToDelete";
        var result = $"User {username} has been deleted.";
        File.WriteAllText($"{username}.json", "{ \"Userame\": \"userToDelete\", \"Password\": \"password\", \"Role\": \"user\" }");

        var inputSequence = new Queue<string>(new[] { username });
        Console.SetIn(new StringReader(string.Join(Environment.NewLine, inputSequence)));

        // Act
        var deleteResult = _userService.DeleteUser(username);

        // Assert
        Assert.Equal(result, deleteResult);

        if (File.Exists($"{username}.json"))
        {
            File.Delete($"{username}.json");
        }
    }
    [Fact]
    public void DeleteUser_ShouldSendFailureMessage_WhenUserDoesNotExist()
    {
        // Arrange
        var username = "nonexistentuser";
        var result = $"User {username} does not exist.";

        var inputSequence = new Queue<string>(new[] { username });
        Console.SetIn(new StringReader(string.Join(Environment.NewLine, inputSequence)));

        // Act
        var deleteResult = _userService.DeleteUser(username);

        // Assert
        Assert.Equal(result, deleteResult);
    }
    
    [Fact]
    public void AddUser_ShouldReturnSuccessMessage_WhenUserIsAddedSuccessfully()
    {
        // Arrange
        var username = "newuser";
        var password = "password123";
        var expectedMessage = $"User {username} has been added.";

        if (File.Exists($"{username}.json"))
        {
            File.Delete($"{username}.json");
        }

        // Act
        var result = _userService.AddUser(username, password);

        // Assert
        Assert.Equal(expectedMessage, result);
        Assert.True(File.Exists($"{username}.json"));

        // Clean up
        if (File.Exists($"{username}.json"))
        {
            File.Delete($"{username}.json");
        }
    }

    [Fact]
    public void AddUser_ShouldReturnErrorMessage_WhenUserAlreadyExists()
    {
        // Arrange
        var username = "existinguser";
        var password = "password123";
        var expectedMessage = $"User {username} already exists.";

        File.WriteAllText($"{username}.json", $"{{ \"Userame\": \"{username}\", \"Password\": \"{password}\", \"Role\": \"user\" }}");

        // Act
        var result = _userService.AddUser(username, password);

        // Assert
        Assert.Equal(expectedMessage, result);

        // Clean up
        if (File.Exists($"{username}.json"))
        {
            File.Delete($"{username}.json");
        }
    }
    
    [Fact]
    public void GetUserInfo_ShouldReturnCorrectUserInfo_WhenUserExists()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";
        var role = "user";
        var expectedUserInfo = $"Username: {username}\nPassword: {password}\nRole: {role}";
        File.WriteAllText($"{username}.json", $"{{ \"Userame\": \"{username}\", \"Password\": \"{password}\", \"Role\": \"{role}\" }}");

        // Act
        var userInfo = _userService.GetUserInfo(username);

        // Assert
        Assert.Equal(expectedUserInfo, userInfo);

        if (File.Exists($"{username}.json"))
        {
            File.Delete($"{username}.json");
        }
    }

    [Fact]
    public void GetUserInfo_ShouldReturnErrorMessage_WhenUserDoesNotExist()
    {
        // Arrange
        var username = "nonexistentuser";
        var expectedMessage = "User file not found.";

        // Act
        var userInfo = _userService.GetUserInfo(username);

        // Assert
        Assert.Equal(expectedMessage, userInfo);
    }
    
    [Fact]
    public void Login_ShouldReturnTrue_WhenCredentialsAreCorrect()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";
        var loginPrompt = JsonConvert.SerializeObject(new { command = "Enter username:" });
        var passwordPrompt = JsonConvert.SerializeObject(new { command = "Enter password:" });
        var loginResponse = JsonConvert.SerializeObject(new { command = "loggedIn" });

        _mockSocketWrapper.Setup(m => m.Receive(It.IsAny<byte[]>()))
            .Callback<byte[]>(buffer => Encoding.ASCII.GetBytes(loginPrompt).CopyTo(buffer, 0))
            .Returns(loginPrompt.Length);

        _mockSocketWrapper.Setup(m => m.Receive(It.IsAny<byte[]>()))
            .Callback<byte[]>(buffer => Encoding.ASCII.GetBytes(passwordPrompt).CopyTo(buffer, 0))
            .Returns(passwordPrompt.Length);

        _mockSocketWrapper.Setup(m => m.Receive(It.IsAny<byte[]>()))
            .Callback<byte[]>(buffer => Encoding.ASCII.GetBytes(loginResponse).CopyTo(buffer, 0))
            .Returns(loginResponse.Length);
        
        var inputSequence = new Queue<string>(new[] { username, password });
        Console.SetIn(new StringReader(string.Join(Environment.NewLine, inputSequence)));

        // Act
        var result = _userService.Login();

        // Assert
        Assert.True(result);
        Assert.Equal(username, _userService.GetLoggedInUser());

        _mockSocketWrapper.Verify(m => m.Send(It.IsAny<byte[]>()), Times.Exactly(2));
    }
    
    [Fact]
    public void Login_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        // Arrange
        var username = "nonexistentuser";
        var password = "password123";
        var loginPrompt = JsonConvert.SerializeObject(new { command = "Enter username:" });
        var passwordPrompt = JsonConvert.SerializeObject(new { command = "Enter password:" });
        var loginResponse = JsonConvert.SerializeObject(new { command = "User doesn't exist." });

        _mockSocketWrapper.Setup(m => m.Receive(It.IsAny<byte[]>()))
            .Callback<byte[]>(buffer => Encoding.ASCII.GetBytes(loginPrompt).CopyTo(buffer, 0))
            .Returns(loginPrompt.Length);

        _mockSocketWrapper.Setup(m => m.Receive(It.IsAny<byte[]>()))
            .Callback<byte[]>(buffer => Encoding.ASCII.GetBytes(passwordPrompt).CopyTo(buffer, 0))
            .Returns(passwordPrompt.Length);

        _mockSocketWrapper.Setup(m => m.Receive(It.IsAny<byte[]>()))
            .Callback<byte[]>(buffer => Encoding.ASCII.GetBytes(loginResponse).CopyTo(buffer, 0))
            .Returns(loginResponse.Length);

        var inputSequence = new Queue<string>(new[] { username, password });
        Console.SetIn(new StringReader(string.Join(Environment.NewLine, inputSequence)));

        // Act
        var result = _userService.Login();

        // Assert
        Assert.False(result);
        Assert.Null(_userService.GetLoggedInUser());

        _mockSocketWrapper.Verify(m => m.Send(It.IsAny<byte[]>()), Times.Exactly(2));
    }

    
    [Fact]
    public void PrintUserInfo_ShouldPrintUserInfo_WhenApproved()
    {
        // Arrange
        var command = "user";
        var approvalResponse = JsonConvert.SerializeObject(new { command = "approved" });
        var userInfoResponse = JsonConvert.SerializeObject(new { command = "User Info: testUser" });

        _mockSocketWrapper.Setup(m => m.Receive(It.IsAny<byte[]>()))
            .Callback<byte[]>(buffer => Encoding.ASCII.GetBytes(approvalResponse).CopyTo(buffer, 0))
            .Returns(approvalResponse.Length);

        _mockSocketWrapper.Setup(m => m.Receive(It.IsAny<byte[]>()))
            .Callback<byte[]>(buffer => Encoding.ASCII.GetBytes(userInfoResponse).CopyTo(buffer, 0))
            .Returns(userInfoResponse.Length);

        var inputSequence = new Queue<string>(new[] { "testUser" });
        Console.SetIn(new StringReader(string.Join(Environment.NewLine, inputSequence)));

        // Act
        _userService.PrintUserInfo(command);

        // Assert
        _mockSocketWrapper.Verify(m => m.Send(It.IsAny<byte[]>()), Times.Exactly(2));
        _mockSocketWrapper.Verify(m => m.Receive(It.IsAny<byte[]>()), Times.Exactly(2));
    }

    [Fact]
    public void PrintUserInfo_ShouldPrintCurrentUserInfo_WhenNotApproved()
    {
        // Arrange
        var command = "user";
        var notApprovedResponse = JsonConvert.SerializeObject(new { command = "not approved" });
        var currentUserResponse = JsonConvert.SerializeObject(new { command = "User Info: currentUser" });

        _mockSocketWrapper.Setup(m => m.Receive(It.IsAny<byte[]>()))
            .Callback<byte[]>(buffer => Encoding.ASCII.GetBytes(notApprovedResponse).CopyTo(buffer, 0))
            .Returns(notApprovedResponse.Length);

        _mockSocketWrapper.Setup(m => m.Receive(It.IsAny<byte[]>()))
            .Callback<byte[]>(buffer => Encoding.ASCII.GetBytes(currentUserResponse).CopyTo(buffer, 0))
            .Returns(currentUserResponse.Length);

        var inputSequence = new Queue<string>(new[] { "currentUser" });
        Console.SetIn(new StringReader(string.Join(Environment.NewLine, inputSequence)));

        // Act
        _userService.PrintUserInfo(command);

        // Assert
        _mockSocketWrapper.Verify(m => m.Send(It.IsAny<byte[]>()), Times.Exactly(2));
        _mockSocketWrapper.Verify(m => m.Receive(It.IsAny<byte[]>()), Times.Exactly(2));
    }
    
}