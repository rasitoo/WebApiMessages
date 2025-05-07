namespace WebApiMessages.Models;

public class Chat
{
    public int Id { get; set; }
    public int CreatorId { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
}
