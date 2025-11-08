namespace DrawebData.TransferObjects;

public class DrawDTO
{
    public int DrawId { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public string Url { get; set; } = string.Empty;
    public DateTime LastUpdate { get; set; }
}