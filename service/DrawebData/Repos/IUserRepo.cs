using DrawebData.Models;

namespace DrawebData.Repos;

public interface IUserRepo
{
    Task<Result<User>> CreateUser(string username, string email, string password);
    Task<Result<bool>> Login(string username, string password);
}
