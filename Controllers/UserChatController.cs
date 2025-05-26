using Microsoft.AspNetCore.Mvc;
using WebApiMessages.Data;
using WebApiMessages.Models.Intermediates;
using WebApiMessages.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
        [FromQuery] int? chatId,
        [FromQuery] string? chatName)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId) || userId == 0)
            return Unauthorized();

        var query = _context.UserChats
            .Include(uc => uc.Chat)
            .Where(uc => uc.UserId == userId);

        if (chatId.HasValue)
            query = query.Where(uc => uc.ChatId == chatId.Value);

        if (!string.IsNullOrEmpty(chatName))
            query = query.Where(uc => uc.Chat.Name.Contains(chatName));

        var userChats = query
            .Select(uc => new UserChatReadDTO
            {
                UserId = uc.UserId,
                ChatId = uc.ChatId,
                Chat = new ChatReadDTO
                {
                    Id = uc.Chat.Id,
                    CreatorId = uc.Chat.CreatorId,
                    Name = uc.Chat.Name,
                    CreatedAt = uc.Chat.CreatedAt
                },
                Users = _context.UserChats
                    .Where(ucc => ucc.ChatId == uc.ChatId)
                    .Select(ucc => new UserInChatDTO { UserId = ucc.UserId })
                    .ToList()
            })
            .ToList();

        return Ok(userChats);
    }



    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateUserChat(UserChatCreateDTO dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId) || userId == 0)
            return Unauthorized();
        var userChat = new User_Chat
        {
            UserId = dto.UserId,
            ChatId = dto.ChatId
        };

        _context.UserChats.Add(userChat);
        await _context.SaveChangesAsync();

        await _hubContext.Clients.Group(dto.ChatId.ToString())
            .SendAsync("UserJoined", new { UserId = dto.UserId, ChatId = dto.ChatId });

        return NoContent();
    }

    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> DeleteUserChat(int userId, int chatId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var currentUserId) || currentUserId == 0)
            return Unauthorized();

        var userChat = _context.UserChats
            .Include(uc => uc.Chat)
            .FirstOrDefault(uc => uc.UserId == userId && uc.ChatId == chatId);

        if (userChat == null)
            return NotFound();

        if (currentUserId != userChat.UserId && currentUserId != userChat.Chat.CreatorId)
            return Forbid();

        await _hubContext.Clients.Group(chatId.ToString())
           .SendAsync("UserLeft", new { UserId = userId, ChatId = chatId });

        _context.UserChats.Remove(userChat);
        _context.SaveChanges();



        return NoContent();
    }

}
