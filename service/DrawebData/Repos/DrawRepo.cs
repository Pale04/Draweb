namespace DrawebData.Repos;

using DrawebData.Models;
using DrawebData.TransferObjects;
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

    public async Task<Result<List<DrawDTO>>> GetDrawsByUserIdWithPagination(int userId, DateTime lastDrawUpdateDate, int pageSize)
    {
        List<DrawDTO> draws = [];

        var user = await _context.Users
            .Include(u => u.Draws)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
        {
            return new Result<List<DrawDTO>>{ Message = $"User not found, with id: {userId}" };
        }
        else
        {
            draws = user.Draws
                .OrderBy(d => d.LastUpdate)
                .Where(d => d.LastUpdate > lastDrawUpdateDate)
                .Take(pageSize)
                .ToList()
                .ConvertAll(new Converter<Draw, DrawDTO>(
                    (draw) =>
                        new()
                        {
                            DrawId = draw.DrawId,
                            UserId = draw.UserId,
                            Title = draw.Title,
                            CreationDate = (DateTime)draw.CreationDate!,
                            Url = draw.Url,
                            LastUpdate = (DateTime)draw.LastUpdate!,
                        }
                ));

                return new Result<List<DrawDTO>>
                {
                    Data = draws,
                    IsSuccess = true,
                    Message = $"Draws from user id: {userId} retrieved with starting date: {lastDrawUpdateDate}"
                };
        }
    }

    //TODO: it might need to store the file too.
    public async Task<Result<DrawDTO>> SaveDraw(int userId, string title, string fileUrl)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user != null)
        {
            Draw newDraw = new()
            {
                Title = title,
                Url = fileUrl
            };
            user.Draws.Add(newDraw);
            int rowsAffected = _context.SaveChanges();

            if (rowsAffected != 1)
            {
                return new Result<DrawDTO>() { Message = $"Failed draw creation of user {userId} with draw location at {fileUrl}" };
            }
            else
            {
                return new Result<DrawDTO>()
                {
                    Data = new()
                    {
                        DrawId = newDraw.DrawId,
                        UserId = newDraw.UserId,
                        Title = newDraw.Title,
                        CreationDate = (DateTime)newDraw.CreationDate!,
                        Url = newDraw.Url,
                        LastUpdate = (DateTime)newDraw.LastUpdate!
                    },
                    IsSuccess = true,
                    Message = $"User with Id {userId} added a new Draw at {fileUrl}"
                };
            }
        }
        else
        {
            return new() { Message = $"Failed draw creation of non-existent user with ID: {userId}" };
        }
    }

    public async Task<Result<DrawDTO>> UpdateDraw(int drawId, string? title)
    {
        var draw = await _context.Draws
            .FirstOrDefaultAsync(d => d.DrawId == drawId);
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

        return new Result<DrawDTO>
        {
            Data = drawExists ? new()
            {
                DrawId = draw!.DrawId,
                UserId = draw.UserId,
                Title = draw.Title,
                CreationDate = (DateTime)draw.CreationDate!,
                Url = draw.Url,
                LastUpdate = (DateTime)draw.LastUpdate!
            } : null,
            IsSuccess = drawExists,
            Message = drawExists ? $"Draw updated with id: {drawId}" : $"Failed updating to non-existent draw with id: {drawId}"
        };
    }
}