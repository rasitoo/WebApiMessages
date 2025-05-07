namespace WebApiMessages.Models.Intermediates;

public class User_Chat
{
    public int UserId { get; set; }
    public int ChatId { get; set; }
    public Chat Chat { get; set; }

}
