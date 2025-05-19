using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using WebApiMessages.Data;
using WebApiMessages.Models;
using WebApiMessages.Models.DTO;

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
    [HttpGet("{id}")]
    public ActionResult<ChatReadDTO> GetChat(int id)
    {
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

        return Ok(chat);
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

        return CreatedAtAction(nameof(GetChat), new { id = chat.Id }, chatReadDTO);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateChat(int id, ChatUpdateDTO dto)
    {
        var chat = _context.Chats.Find(id);
        if (chat == null)
            return NotFound();

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
        var chat = _context.Chats.Find(id);
        if (chat == null)
            return NotFound();

        _context.Chats.Remove(chat);
        _context.SaveChanges();

        await _hubContext.Clients.Group(id.ToString()).SendAsync("ChatDeleted", id);

        return NoContent();
    }
}
