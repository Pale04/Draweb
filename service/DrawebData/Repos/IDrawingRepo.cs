using DrawebData.TransferObjects;
using DrawebData.Helpers;

namespace DrawebData.Repos;

public interface IDrawingRepo
{
    Task<Result<List<DrawingDTO>>> GetDrawsByUserIdWithPagination(int userId, DateTime lastDrawUpdateDate, int pageSize);
    Task<Result<string>> GetSvgDrawing(int drawingId);
    Task<Result<DrawingDTO>> SaveDraw(int userId, string title, string svgStructure);
    Task<Result<bool>> DeleteDrawing(int drawingId);
    Task<Result<DrawingDTO>> UpdateDraw(int drawId, string? title);
}