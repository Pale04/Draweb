using DrawebData.TransferObjects;

namespace DrawebData.Repos;

public interface IUserRepo
{
    Task<Result<UserDTO>> CreateUser(string username, string email, string password);
    Task<Result<bool>> Login(string username, string password);
}
