using VanGest.Server.Models.Filters;

public class ChatMessage
{
    public string Content { get; set; } = string.Empty;
    public bool IsUserMessage { get; set; }
    public VanFilter? Filters { get; set; }  // Rinominato da Filter a Filters per coerenza
}