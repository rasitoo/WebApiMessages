using Microsoft.EntityFrameworkCore;
using WebApiMessages.Models;

namespace WebApiMessages.Data;

public class MessageContext : DbContext
{
    public MessageContext(DbContextOptions<MessageContext> options) : base(options)
    {

    }
    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Message>()
            .Property(e => e.SentAt)
            .HasDefaultValueSql("NOW()");
    }
}
