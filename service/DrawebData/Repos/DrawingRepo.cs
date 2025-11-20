namespace DrawebData.Repos;

using DrawebData.Models;
using DrawebData.TransferObjects;
using DrawebData.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Text;

public class DrawingRepo(DrawebDbContext context) : IDrawingRepo
{
    private readonly DrawebDbContext _context = context;
    private readonly string _drawingsFolder = Path.Combine("../", "Drawings");

    public async Task<Result<DrawingDTO>> SaveDraw(int userId, string title, string svgStructure)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user != null)
        {
            if (!Directory.Exists(_drawingsFolder))
            {
                Directory.CreateDirectory(_drawingsFolder);
            }
            string fileName = $"{Guid.NewGuid()}.drawing.svg";
            string filePath = Path.Combine(_drawingsFolder, fileName);
            await File.WriteAllTextAsync(filePath, svgStructure, Encoding.UTF8);

            Draw newDraw = new()
            {
                Title = title,
                Url = fileName
            };

            user.Draws.Add(newDraw);
            int rowsAffected = _context.SaveChanges();
            if (rowsAffected != 1)
            {
                return new Result<DrawingDTO>() { 
                    Message = $"Failed draw creation of user {userId} with draw location at {filePath}",
                    ErrorType = ErrorType.FailedOperationExecution
                };
            }
            else
            {
                return new Result<DrawingDTO>()
                {
                    Data = new()
                    {
                        DrawingId = newDraw.DrawId,
                        UserId = newDraw.UserId,
                        Title = newDraw.Title,
                        CreationDate = (DateTime)newDraw.CreationDate!,
                        Url = newDraw.Url,
                        LastUpdate = (DateTime)newDraw.LastUpdate!
                    },
                    IsSuccess = true,
                    Message = $"User with Id {userId} added a new Draw at {filePath}"
                };
            }
        }
        else
        {
            return new() { 
                Message = $"Failed draw creation of non-existent user with ID: {userId}, drawing title: {title}, SVG: {svgStructure}",
                ErrorType = ErrorType.ResourceDoesNotExist
            };
        }
    }

    public async Task<Result<List<DrawingDTO>>> GetDrawsByUserIdWithPagination(int userId, DateTime lastDrawingUpdate, int pageSize)
    {
        List<DrawingDTO> draws = [];

        var user = await _context.Users
            .Include(u => u.Draws)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
        {
            return new Result<List<DrawingDTO>>{ 
                Message = $"User not found, with id: {userId}",
                ErrorType = ErrorType.ResourceDoesNotExist
            };
        }
        else
        {
            draws = user.Draws
                .OrderBy(d => d.LastUpdate)
                .Where(d => d.LastUpdate > lastDrawingUpdate)
                .Take(pageSize)
                .ToList()
                .ConvertAll(new Converter<Draw, DrawingDTO>(
                    (draw) =>
                        new()
                        {
                            DrawingId = draw.DrawId,
                            UserId = draw.UserId,
                            Title = draw.Title,
                            CreationDate = (DateTime)draw.CreationDate!,
                            Url = draw.Url,
                            LastUpdate = (DateTime)draw.LastUpdate!,
                        }
                ));

                return new Result<List<DrawingDTO>>
                {
                    Data = draws,
                    IsSuccess = true,
                    Message = $"Draws from user id: {userId} retrieved with starting date: {lastDrawingUpdate}"
                };
        }
    }

    public async Task<Result<string>> GetSvgDrawing(int drawingId)
    {
        var drawing = await _context.Draws
            .FirstOrDefaultAsync(d => d.DrawId == drawingId);

        if(drawing == null)
        {
            return new()
            {
                Message = $"A non-existent drawing was searched with ID: {drawingId}",
                ErrorType = ErrorType.ResourceDoesNotExist
            };
        }

        string filePath = Path.Combine(_drawingsFolder, drawing.Url);
        if (!File.Exists(filePath))
        {
            return new()
            {
                Message = $"A drawing without svg file was searched with ID: {drawingId}",
                ErrorType = ErrorType.FailedOperationExecution
            };
        }
        string svg = File.ReadAllText(filePath, Encoding.UTF8);
        
        return new()
        {
            Data = svg,
            Message = $"The svg file of a drawing was searched, with ID: {drawingId}",
            IsSuccess = true   
        };
    }

    public async Task<Result<bool>> DeleteDrawing(int drawingId)
    {
        var drawing = await _context.Draws
            .FirstOrDefaultAsync(d => d.DrawId == drawingId);

        if (drawing is null)
        {
            return new()
            {
                Message = $"Attempted to delete a non-existent drawing with ID: {drawingId}",
                ErrorType = ErrorType.ResourceDoesNotExist
            };
        }

        string filePath = Path.Combine(_drawingsFolder, drawing.Url);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }        

        _context.Draws.Remove(drawing);
        await _context.SaveChangesAsync();
        return new Result<bool>
        {
            Data = true,
            IsSuccess = true,
            Message = $"Deleted drawing with ID: {drawingId}"
        }; 
    }

    public async Task<Result<DrawingDTO>> UpdateDraw(int drawId, string? title)
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

        return new Result<DrawingDTO>
        {
            Data = drawExists ? new()
            {
                DrawingId = draw!.DrawId,
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