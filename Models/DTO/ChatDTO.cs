namespace WebApiMessages.Models.DTO;

public class ChatReadDTO
{
    public str Id { get; set; }
    public int CreatorId { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ChatCreateDTO
{
    public string Name { get; set; }
}

public class ChatUpdateDTO
{
    public string Name { get; set; }
}
