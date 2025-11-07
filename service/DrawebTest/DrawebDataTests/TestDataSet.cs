namespace DrawebTest.DrawebDataTests;

using DrawebData.Models;

public class TestDataSet
{
    public static readonly User RegisteredUser = new()
    {
        UserId = 1,
        Username = "testuser1",
        Email = "testuser1@example.com",
        Password = "securepassword"
    };

    public static readonly List<Draw> UserDraws = new()
    {
        new Draw
        {   
            DrawId = 1,
            UserId = RegisteredUser.UserId,
            Title = "Sunset",
            Url = "/draws/sunset.png"
        },
        new Draw
        {
            DrawId = 2,
            UserId = RegisteredUser.UserId,
            Title = "Mountain",
            Url = "/draws/mountain.png"
        },
        new Draw
        {
            DrawId = 3,
            UserId = RegisteredUser.UserId,
            Title = "Ocean",
            Url = "/draws/ocean.png"
        }
    };
}
