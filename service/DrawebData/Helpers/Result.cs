namespace DrawebData.Helpers;

public record class Result<T>
{
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the operations was completed without errors.
    /// </summary>
    public bool IsSuccess { get; set; } = false;

    public ErrorType ErrorType { get; set; } = ErrorType.None;
}