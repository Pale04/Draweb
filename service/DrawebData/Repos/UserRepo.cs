namespace DrawebData.Repos;

using DrawebData.Models;
using DrawebData.TransferObjects;
using DrawebData.Helpers;
using Microsoft.EntityFrameworkCore;

public class UserRepo(DrawebDbContext context) : IUserRepo
{
    private readonly DrawebDbContext _context = context;

    public async Task<Result<UserDTO>> CreateUser(string username, string email, string password)
    {
        int rowsAffected = 0;
        UserDTO insertedUser;

        bool userExists = await _context.Users.AnyAsync(u => u.Username == username || u.Email == email);
        if (!userExists)
        {
            User newUser = new()
            {
                Username = username,
                Email = email,
                Password = password
            };
            await _context.Users.AddAsync(newUser);
            rowsAffected = await _context.SaveChangesAsync();
            insertedUser = new()
            {
                Id = newUser.UserId,
                Username = newUser.Username,
                Email = newUser.Email,
            };
        }
        else
        {
            return new Result<UserDTO>{
                Message = "Attempt of user registrations with existent username or email.",
                ErrorType = ErrorType.UserAlreadyExists
            };
        }

        if (rowsAffected == 1)
        {
            return new Result<UserDTO>
            {
                Data = insertedUser,
                IsSuccess = true,
                Message = $"User {insertedUser.Username} created with ID {insertedUser.Id}"
            };
        }
        else
        {
            return new Result<UserDTO>{
                Message = $"Failed to create user: {rowsAffected} rows affected.",
                ErrorType = ErrorType.FailedOperationExecution
            };
        }
    }

    public async Task<Result<UserDTO>> Login(string username, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

        Result<UserDTO> result = new()
        {
            Data = user == null 
                ? null 
                : new()
                {
                    Id = user.UserId,
                    Username = user.Username,
                    Email = user.Email
                },
            IsSuccess = user != null,
            Message = user == null ? $"Failed login with username: {username}." : $"Login successful of user: {username}."
        };

        return result;
    }
}