namespace DrawebTest.DrawebDataTests;

using DrawebData.Repos;

public class DrawRepoTest : IClassFixture<TestDatabaseFixture>
{
    public TestDatabaseFixture Fixture { get; }

    public DrawRepoTest(TestDatabaseFixture fixture) => Fixture = fixture;

    [Fact]
    public async Task GetDrawsByUserIdWithPagination_ShouldReturnDrawsSuccessfully()
    {
        var repo = new DrawRepo(Fixture.CreateContext());
        int userId = 1;
        DateTime lastDrawUpdateDate = DateTime.MinValue;
        int pageSize = TestDataSet.UserDraws.Count;

        var result = await repo.GetDrawsByUserIdWithPagination(userId, lastDrawUpdateDate, pageSize);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Count > 0, $"{result.Data.Count} != {TestDataSet.UserDraws.Count}");
    }

    [Fact]
    public async Task GetDrawsByUserIdWithPagination_ShouldReturnUserNotFound()
    {
        var repo = new DrawRepo(Fixture.CreateContext());
        int userId = 100;
        DateTime lastDrawUpdateDate = DateTime.MinValue;
        int pageSize = 10;

        var result = await repo.GetDrawsByUserIdWithPagination(userId, lastDrawUpdateDate, pageSize);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task SaveDraw_ShouldReturnSuccessfulOperation()
    {
        var repository = new DrawRepo(Fixture.CreateContext());
        int userId = TestDataSet.RegisteredUser.UserId;
        string title = "TestDraw";
        string url = "/draws/1";

        var result = await repository.SaveDraw(userId, title, url);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task SaveDraw_ShouldReturnUserNotFound()
    {
        var repository = new DrawRepo(Fixture.CreateContext());
        int userId = 100;
        string title = "TestDraw";
        string url = "/draws/1";

        var result = await repository.SaveDraw(userId, title, url);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task DeleteDraw_ShouldReturnSuccessfulResult()
    {
        var repository = new DrawRepo(Fixture.CreateContext());
        int drawId = TestDataSet.UserDraws[2].DrawId;

        var result = await repository.DeleteDraw(drawId);

        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task DeleteDraw_ShouldReturnDrawNotFound()
    {
        var repository = new DrawRepo(Fixture.CreateContext());
        int drawId = 100;

        var result = await repository.DeleteDraw(drawId);

        Assert.False(result.IsSuccess);
        Assert.False(result.Data);
    }

    [Fact]
    public async Task UpdateDraw_ShouldReturnSuccessfulResult()
    {
        using var context = Fixture.CreateContext();
        var repository = new DrawRepo(context);
        int drawId = TestDataSet.UserDraws[1].DrawId;
        string title = "TestDraw";
        int userId = TestDataSet.UserDraws[1].UserId;

        var userDraws = await repository.GetDrawsByUserIdWithPagination(userId, DateTime.MinValue, TestDataSet.UserDraws.Count);
        DateTime? previousUpdateTime = userDraws.Data!.First(d => d.DrawId == drawId).LastUpdate;

        await Task.Delay(2000);
        var result = await repository.UpdateDraw(drawId, title);

        var updatedUserDraws = await repository.GetDrawsByUserIdWithPagination(userId, DateTime.MinValue, TestDataSet.UserDraws.Count);
        DateTime? newUpdateTime = updatedUserDraws.Data!.First(d => d.DrawId == drawId).LastUpdate;

        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
        Assert.Equal(updatedUserDraws.Data!.First(d => d.DrawId == drawId).Title, title);
        Assert.True(previousUpdateTime!.Value.CompareTo(newUpdateTime!.Value) < 0, $"Previous = {previousUpdateTime.Value}, Now = {newUpdateTime.Value}");
    }
    
    [Fact]
    public async Task UpdateDraw_ShouldReturnDrawNotFound()
    {
        var repository = new DrawRepo(Fixture.CreateContext());
        int drawId = 100;
        string title = "TestDraw";

        var result = await repository.UpdateDraw(drawId, title);

        Assert.False(result.IsSuccess);
        Assert.False(result.Data);
    }
}