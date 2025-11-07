namespace DrawebData.Repos;

using DrawebData.Models;
using Microsoft.EntityFrameworkCore;

public class DrawRepo(DrawebDbContext context) : IDrawRepo
{
    private readonly DrawebDbContext _context = context;

    public async Task<Result<bool>> DeleteDraw(int drawId)
    {
        bool deleted = false;

        var draw = await _context.Draws
            .FirstOrDefaultAsync(d => d.DrawId == drawId);

        if (draw != null)
        {
            _context.Draws.Remove(draw);
            await _context.SaveChangesAsync();
            deleted = true;
        }

        return new Result<bool>
        {
            Data = deleted,
            IsSuccess = deleted,
            Message = deleted ? $"Draw deleted, with id: {drawId}" : $"Draw not found, with id: {drawId}"
        }; 
    }

    public async Task<Result<List<Draw>>> GetDrawsByUserIdWithPagination(int userId, DateTime lastDrawUpdateDate, int pageSize)
    {
        List<Draw> draws = [];

        var user = await _context.Users
            .Include(u => u.Draws)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
        {
            return new Result<List<Draw>>
            {
                Message = $"User not found, with id: {userId}",
                IsSuccess = false
            };
        }
        else
        {
            draws = user.Draws
                .OrderBy(d => d.LastUpdate)
                .Where(d => d.LastUpdate > lastDrawUpdateDate)
                .Take(pageSize)
                .ToList();
        }

        return new Result<List<Draw>>
        {
            Data = draws,
            IsSuccess = true,
            Message = $"Draws from user id: {userId} retrieved with starting date: {lastDrawUpdateDate}"
        };
    }

    //TODO: it might need to store the file too.
    public async Task<Result<Draw>> SaveDraw(int userId, string title, string fileUrl)
    {
        Draw newDraw = new()
        {
            //UserId = userId,
            Title = title,
            Url = fileUrl
        };
        int rowsAffected = 0;

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user != null)
        {
            user.Draws.Add(newDraw);
            rowsAffected = _context.SaveChanges();
        }        

        if (rowsAffected != 1)
        {
            return new Result<Draw>()
            {
                Message = $"Failed draw creation of user {userId} with draw location at {fileUrl}"
            };
        }

        return new Result<Draw>()
        {
            Data = newDraw,
            IsSuccess = true,
            Message = $"User with Id {userId} added a new Draw at {fileUrl}"
        };
    }

    public async Task<Result<bool>> UpdateDraw(int drawId, string? title)
    {
        var draw = await _context.Draws.FirstOrDefaultAsync(d => d.DrawId == drawId);
        bool drawExists = draw != null;

        if (drawExists)
        {
            if (title != null)
            {
                draw!.Title = title;
                await _context.SaveChangesAsync();
            }
            draw!.LastUpdate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return new Result<bool>
        {
            Data = drawExists,
            IsSuccess = drawExists,
            Message = drawExists ? $"Draw updated with id: {drawId}" : $"Failed updating to non-existent draw with id: {drawId}"
        };
    }
}