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
public class MessagesController : ControllerBase
{
    private readonly MessageContext _context;
    private readonly IHubContext<MessageHub> _hubContext;

    public MessagesController(MessageContext context, IHubContext<MessageHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    [Authorize]
    [HttpGet]
    public ActionResult<IEnumerable<MessageReadDTO>> GetMessages(
        [FromQuery] int? chatId,
        [FromQuery] int? senderId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var query = _context.Messages.AsQueryable();

        if (chatId.HasValue)
            query = query.Where(m => m.ChatId == chatId.Value);

        if (senderId.HasValue)
            query = query.Where(m => m.SenderId == senderId.Value);

        if (startDate.HasValue)
            query = query.Where(m => m.SentAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(m => m.SentAt <= endDate.Value);

        var messages = query
            .Select(m => new MessageReadDTO
            {
                Id = m.Id,
                ChatId = m.ChatId,
                SenderId = m.SenderId,
                Content = m.Content,
                SentAt = m.SentAt
            })
            .ToList();

        return Ok(messages);
    }

    [Authorize]
    [HttpGet("{id}")]
    public ActionResult<MessageReadDTO> GetMessage(int id)
    {
        var message = _context.Messages
            .Where(m => m.Id == id)
            .Select(m => new MessageReadDTO
            {
                Id = m.Id,
                ChatId = m.ChatId,
                SenderId = m.SenderId,
                Content = m.Content,
                SentAt = m.SentAt
            })
            .FirstOrDefault();

        if (message == null)
            return NotFound();

        return Ok(message);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<MessageReadDTO>> CreateMessage(MessageCreateDTO dto)
    {
        var message = new Message
        {
            ChatId = dto.ChatId,
            SenderId = int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var creatorId) ? creatorId : 0,
            Content = dto.Content,
            SentAt = DateTime.UtcNow
        };

        _context.Messages.Add(message);
        _context.SaveChanges();

        var messageReadDTO = new MessageReadDTO
        {
            Id = message.Id,
            ChatId = message.ChatId,
            SenderId = message.SenderId,
            Content = message.Content,
            SentAt = message.SentAt
        };

        await _hubContext.Clients.Group(message.ChatId.ToString()).SendAsync("ReceiveMessage", messageReadDTO);

        return CreatedAtAction(nameof(GetMessage), new { id = message.Id }, messageReadDTO);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMessage(int id, MessageUpdateDTO dto)
    {
        var message = _context.Messages.Find(id);
        if (message == null)
            return NotFound();

        message.Content = dto.Content;
        _context.SaveChanges();

        var messageReadDTO = new MessageReadDTO
        {
            Id = message.Id,
            ChatId = message.ChatId,
            SenderId = message.SenderId,
            Content = message.Content,
            SentAt = message.SentAt
        };

        await _hubContext.Clients.Group(message.ChatId.ToString()).SendAsync("MessageUpdated", messageReadDTO);

        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMessage(int id)
    {
        var message = _context.Messages.Find(id);
        if (message == null)
            return NotFound();

        int chatId = message.ChatId;

        _context.Messages.Remove(message);
        _context.SaveChanges();

        await _hubContext.Clients.Group(chatId.ToString()).SendAsync("MessageDeleted", id);

        return NoContent();
    }
}
