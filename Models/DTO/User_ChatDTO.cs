namespace WebApiMessages.Models.DTO;
public class UserChatReadDTO
{
    public int UserId { get; set; }
    public int ChatId { get; set; }
    public ChatReadDTO Chat { get; set; }
    public List<UserInChatDTO> Users { get; set; } 
}


public class UserChatCreateDTO
{
    public int UserId { get; set; }
    public int ChatId { get; set; }
}

