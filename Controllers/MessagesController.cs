using Microsoft.AspNetCore.Mvc;
using WebApiMessages.Data;
using WebApiMessages.Models;
using WebApiMessages.Models.DTO;

namespace WebApiMessages.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessageController : ControllerBase
{
    private readonly MessageContext _context;

    public MessageController(MessageContext context)
    {
        _context = context;
    }

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

    [HttpPost]
    public ActionResult<MessageReadDTO> CreateMessage(MessageCreateDTO dto)
    {
        var message = new Message
        {
            ChatId = dto.ChatId,
            SenderId = dto.SenderId,
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

        return CreatedAtAction(nameof(GetMessage), new { id = message.Id }, messageReadDTO);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateMessage(int id, MessageUpdateDTO dto)
    {
        var message = _context.Messages.Find(id);
        if (message == null)
            return NotFound();

        message.Content = dto.Content;
        _context.SaveChanges();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteMessage(int id)
    {
        var message = _context.Messages.Find(id);
        if (message == null)
            return NotFound();

        _context.Messages.Remove(message);
        _context.SaveChanges();

        return NoContent();
    }
}
