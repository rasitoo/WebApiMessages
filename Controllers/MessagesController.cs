using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiMessages.Data;
using WebApiMessages.Models;

namespace WebApiMessages.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly MessageContext _context;
    public MessagesController(MessageContext messageContext)
    {
        _context = messageContext;
    }
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Message>>> GetMessages()
    {
        return await _context.Messages.ToListAsync();
    }
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Message>> PostMessage(Message message)
    {
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetMessages), new { id = message.Id }, message);
    }
}
