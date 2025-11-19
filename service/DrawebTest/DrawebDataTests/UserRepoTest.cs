namespace DrawebTest.DrawebDataTests;

using DrawebData.Repos;

public class UserRepoTest : IClassFixture<TestDatabaseFixture>
{
    public TestDatabaseFixture Fixture { get; }

    public UserRepoTest(TestDatabaseFixture fixture) => Fixture = fixture;

    [Fact]
    public async Task AddUser_ShouldCreateUserSuccessfully()
    {
        using var context = Fixture.CreateContext();
        var repo = new UserRepo(context);
        string username = "newuser";
        string email = "newuser@example.com";
        string password = "securepassword";

        var result = await repo.CreateUser(username, email, password);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task AddUser_ShouldFailWhenUsernameExists()
    {
        using var context = Fixture.CreateContext();
        var repo = new UserRepo(context);
        string username = TestDataSet.RegisteredUser.Username;
        string email = "example@example.com";
        string password = "securepassword";

        var result = await repo.CreateUser(username, email, password);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task AddUser_ShouldFailWhenEmailExists()
    {
        using var context = Fixture.CreateContext();
        var repo = new UserRepo(context);
        string username = "exampleUser";
        string email = TestDataSet.RegisteredUser.Email;
        string password = "securepassword";

        var result = await repo.CreateUser(username, email, password);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Login_ShouldSucceedWithValidCredentials()
    {
        using var context = Fixture.CreateContext();
        var repo = new UserRepo(context);
        string username = TestDataSet.RegisteredUser.Username;
        string password = TestDataSet.RegisteredUser.Password;

        var result = await repo.Login(username, password);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Login_ShouldFailWithInvalidCredentials()
    {
        using var context = Fixture.CreateContext();
        var repo = new UserRepo(context);
        string username = "invalidUser";
        string password = "invalidPassword";

        var result = await repo.Login(username, password);

        Assert.False(result.IsSuccess);
    }
}

