using Microsoft.EntityFrameworkCore;
using WebApiMessages.Models;
using WebApiMessages.Models.Intermediates;

namespace WebApiMessages.Data
{
    public class MessageContext : DbContext
    {
        public MessageContext(DbContextOptions<MessageContext> options) : base(options)
        {
        }

        public DbSet<Message> Messages { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<User_Chat> UserChats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Message>(entity =>
            {
                entity.ToTable("Messages");
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Id)
                    .ValueGeneratedOnAdd();
                entity.Property(m => m.ChatId)
                    .IsRequired();
                entity.Property(m => m.SenderId)
                    .IsRequired();
                entity.Property(m => m.Content)
                    .IsRequired()
                    .HasColumnType("text");
                entity.Property(m => m.SentAt)
                    .IsRequired()
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql("NOW()");
            });

            modelBuilder.Entity<Chat>(entity =>
            {
                entity.ToTable("Chats");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id)
                    .ValueGeneratedOnAdd();
                entity.Property(c => c.CreatorId)
                    .IsRequired();
                entity.Property(c => c.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(c => c.CreatedAt)
                    .IsRequired()
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql("NOW()");
            });

            modelBuilder.Entity<User_Chat>(entity =>
            {
                entity.ToTable("User_Chat");
                entity.HasKey(uc => new { uc.UserId, uc.ChatId });
                entity.HasOne(uc => uc.Chat)
                    .WithMany()
                    .HasForeignKey(uc => uc.ChatId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
