using DrawebData.Models;

namespace DrawebData.Repos;

public interface IDrawRepo
{
    Task<Result<List<Draw>>> GetDrawsByUserIdWithPagination(int userId, DateTime lastDrawUpdateDate, int pageSize);
    Task<Result<Draw>> SaveDraw(int userId, string title, string fileUrl);
    Task<Result<bool>> DeleteDraw(int drawId);
    Task<Result<bool>> UpdateDraw(int drawId, string? title);
}