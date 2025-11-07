namespace DrawebData.Repos;

using DrawebData.Models;
using Microsoft.EntityFrameworkCore;

public class UserRepo(DrawebDbContext context) : IUserRepo
{
    private readonly DrawebDbContext _context = context;

    public async Task<Result<User>> CreateUser(string username, string email, string password)
    {
        int rowsAffected = 0;
        User newUser = new()
        {
            Username = username,
            Email = email,
            Password = password
        };
        Result<User> result;

        bool userExists = await _context.Users.AnyAsync(u => u.Username == username || u.Email == email);
        if (!userExists)
        {
            await _context.Users.AddAsync(newUser);
            rowsAffected = await _context.SaveChangesAsync();
        }

        if (userExists)
        {
            result = new Result<User>
            {
                IsSuccess = false,
                Message = "Username or email already exists."
            };
        }
        else if (rowsAffected == 1)
        {
            result = new Result<User>
            {
                Data = newUser,
                IsSuccess = true,
                Message = $"User {newUser.Username} created"
            };
        }
        else
        {
            result = new Result<User>
            {
                IsSuccess = false,
                Message = "Failed to create user."
            };
        }
        
        return result;
    }

    public async Task<Result<bool>> Login(string username, string password)
    {
        bool correctLogin = await _context.Users.AnyAsync(u => u.Username == username && u.Password == password);

        Result<bool> result = new()
        {
            Data = correctLogin,
            IsSuccess = correctLogin,
            Message = correctLogin ? $"Login successful of user: {username}." : $"Failed login with username: {username}."
        };

        return result;
    }
}