using Microsoft.AspNetCore.Mvc;
using WebApiMessages.Data;
using WebApiMessages.Models.Intermediates;
using WebApiMessages.Models.DTO;

namespace WebApiMessages.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserChatController : ControllerBase
{
    private readonly MessageContext _context;

    public UserChatController(MessageContext context)
    {
        _context = context;
    }

    [HttpGet]
    public ActionResult<IEnumerable<UserChatReadDTO>> GetUserChats(
        [FromQuery] int? userId,
        [FromQuery] int? chatId)
    {
        var query = _context.UserChats.AsQueryable();

        if (userId.HasValue)
            query = query.Where(uc => uc.UserId == userId.Value);

        if (chatId.HasValue)
            query = query.Where(uc => uc.ChatId == chatId.Value);

        var userChats = query
            .Select(uc => new UserChatReadDTO
            {
                UserId = uc.UserId,
                ChatId = uc.ChatId
            })
            .ToList();

        return Ok(userChats);
    }


    [HttpPost]
    public IActionResult CreateUserChat(UserChatCreateDTO dto)
    {
        var userChat = new User_Chat
        {
            UserId = dto.UserId,
            ChatId = dto.ChatId
        };

        _context.UserChats.Add(userChat);
        _context.SaveChanges();

        return NoContent();
    }

    [HttpDelete]
    public IActionResult DeleteUserChat(int userId, int chatId)
    {
        var userChat = _context.UserChats
            .FirstOrDefault(uc => uc.UserId == userId && uc.ChatId == chatId);

        if (userChat == null)
            return NotFound();

        _context.UserChats.Remove(userChat);
        _context.SaveChanges();

        return NoContent();
    }
}
