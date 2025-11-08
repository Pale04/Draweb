using DrawebData.TransferObjects;

namespace DrawebData.Repos;

public interface IDrawRepo
{
    Task<Result<List<DrawDTO>>> GetDrawsByUserIdWithPagination(int userId, DateTime lastDrawUpdateDate, int pageSize);
    Task<Result<DrawDTO>> SaveDraw(int userId, string title, string fileUrl);
    Task<Result<bool>> DeleteDraw(int drawId);
    Task<Result<DrawDTO>> UpdateDraw(int drawId, string? title);
}