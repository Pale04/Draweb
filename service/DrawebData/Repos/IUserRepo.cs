using DrawebData.TransferObjects;
using DrawebData.Helpers;

namespace DrawebData.Repos;

public interface IUserRepo
{
    Task<Result<UserDTO>> CreateUser(string username, string email, string password);
    Task<Result<UserDTO>> Login(string username, string password);
}
