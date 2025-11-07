namespace DrawebTest;

using DrawebData.Models;
using DrawebTest.DrawebDataTests;
using Microsoft.EntityFrameworkCore;

public class TestDatabaseFixture
{
    private const string ConnectionString = "yourConnectionString";

    private static readonly object _lock = new();
    private static bool _databaseInitialized;

    public TestDatabaseFixture()
    {
        lock (_lock)
        {
            if (!_databaseInitialized)
            {
                using (var context = CreateContext())
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();

                    context.AddRange(
                        TestDataSet.RegisteredUser
                    );
                    context.SaveChanges();
                    context.AddRange(
                        TestDataSet.UserDraws
                    );
                    context.SaveChanges();
                    
                    _databaseInitialized = true;
                }
            }
        }
    }

    public DrawebDbContext CreateContext()
        => new DrawebDbContext(
            new DbContextOptionsBuilder<DrawebDbContext>()
                .UseMySQL(ConnectionString)
                .Options);
}
