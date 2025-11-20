namespace DrawebData.TransferObjects;

public record class DrawingDTO
{
    public int DrawingId { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public string Url { get; set; } = string.Empty;
    public DateTime LastUpdate { get; set; }
}