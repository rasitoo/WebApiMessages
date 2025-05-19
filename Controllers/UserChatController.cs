using Microsoft.AspNetCore.Mvc;
using WebApiMessages.Data;
using WebApiMessages.Models.Intermediates;
using WebApiMessages.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace WebApiMessages.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserChatController : ControllerBase
{
    private readonly MessageContext _context;
    private readonly IHubContext<MessageHub> _hubContext;

    public UserChatController(MessageContext context, IHubContext<MessageHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    [Authorize]
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

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateUserChat(UserChatCreateDTO dto)
    {
        var userChat = new User_Chat
        {
            UserId = dto.UserId,
            ChatId = dto.ChatId
        };

        _context.UserChats.Add(userChat);
        _context.SaveChanges();

        await _hubContext.Clients.Group(dto.ChatId.ToString())
            .SendAsync("UserJoinedChat", new { UserId = dto.UserId, ChatId = dto.ChatId });

        return NoContent();
    }

    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> DeleteUserChat(int userId, int chatId)
    {
        var userChat = _context.UserChats
            .FirstOrDefault(uc => uc.UserId == userId && uc.ChatId == chatId);

        if (userChat == null)
            return NotFound();

        _context.UserChats.Remove(userChat);
        _context.SaveChanges();

        await _hubContext.Clients.Group(chatId.ToString())
            .SendAsync("UserLeftChat", new { UserId = userId, ChatId = chatId });

        return NoContent();
    }
}
