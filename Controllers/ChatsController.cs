using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using WebApiMessages.Data;
using WebApiMessages.Models;
using WebApiMessages.Models.DTO;
using WebApiMessages.Models.Intermediates;

namespace WebApiMessages.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatsController : ControllerBase
{
    private readonly MessageContext _context;
    private readonly IHubContext<MessageHub> _hubContext;

    public ChatsController(MessageContext context, IHubContext<MessageHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    [Authorize]
    [HttpGet]
    public ActionResult<IEnumerable<ChatReadDTO>> GetChats(
        [FromQuery] int? creatorId,
        [FromQuery] string? name,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId) || userId == 0)
            return Unauthorized();

        var query = _context.Chats.AsQueryable();

        if (creatorId.HasValue)
            query = query.Where(c => c.CreatorId == creatorId.Value);

        if (!string.IsNullOrEmpty(name))
            query = query.Where(c => c.Name.Contains(name));

        if (startDate.HasValue)
            query = query.Where(c => c.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(c => c.CreatedAt <= endDate.Value);

        var chats = query
            .Where(c => c.CreatorId == userId)
            .Select(c => new ChatReadDTO
            {
                Id = c.Id,
                CreatorId = c.CreatorId,
                Name = c.Name,
                CreatedAt = c.CreatedAt
            })
            .ToList();

        return Ok(chats);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ChatReadDTO>> CreateChat(ChatCreateDTO dto)
    {
        var chat = new Chat
        {
            CreatorId = int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var creatorId) ? creatorId : 0,
            Name = dto.Name,
            CreatedAt = DateTime.UtcNow
        };

        _context.Chats.Add(chat);
        _context.SaveChanges();

        var chatReadDTO = new ChatReadDTO
        {
            Id = chat.Id,
            CreatorId = chat.CreatorId,
            Name = chat.Name,
            CreatedAt = chat.CreatedAt
        };

        await _hubContext.Clients.Group(chat.Id.ToString()).SendAsync("ChatCreated", chatReadDTO);

        var userChat = new User_Chat
        {
            UserId = chat.CreatorId,
            ChatId = chat.Id
        };

        _context.UserChats.Add(userChat);
        _context.SaveChanges();

        await _hubContext.Clients.Group(chat.Id.ToString())
            .SendAsync("UserJoined", new { UserId = chat.CreatorId, ChatId = chat.Id });


        return CreatedAtAction(nameof(GetChat), new { id = chat.Id }, chatReadDTO);
    }

    [Authorize]
    [HttpGet("{id}")]
    public ActionResult<ChatReadDTO> GetChat(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId) || userId == 0)
            return Unauthorized();

        var chat = _context.Chats
            .Where(c => c.Id == id)
            .Select(c => new ChatReadDTO
            {
                Id = c.Id,
                CreatorId = c.CreatorId,
                Name = c.Name,
                CreatedAt = c.CreatedAt
            })
            .FirstOrDefault();

        if (chat == null)
            return NotFound();

        if (chat.CreatorId != userId)
            return Forbid();

        return Ok(chat);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateChat(int id, ChatUpdateDTO dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId) || userId == 0)
            return Unauthorized();

        var chat = _context.Chats.Find(id);
        if (chat == null)
            return NotFound();

        if (chat.CreatorId != userId)
            return Forbid();

        chat.Name = dto.Name;
        _context.SaveChanges();

        var chatReadDTO = new ChatReadDTO
        {
            Id = chat.Id,
            CreatorId = chat.CreatorId,
            Name = chat.Name,
            CreatedAt = chat.CreatedAt
        };

        await _hubContext.Clients.Group(chat.Id.ToString()).SendAsync("ChatUpdated", chatReadDTO);

        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteChat(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId) || userId == 0)
            return Unauthorized();

        var chat = _context.Chats.Find(id);
        if (chat == null)
            return NotFound();

        if (chat.CreatorId != userId)
            return Forbid();

        _context.Chats.Remove(chat);
        _context.SaveChanges();

        await _hubContext.Clients.Group(id.ToString()).SendAsync("ChatDeleted", id);

        return NoContent();
    }

}
