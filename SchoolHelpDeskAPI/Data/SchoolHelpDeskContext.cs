using Microsoft.EntityFrameworkCore;
using SchoolHelpDeskAPI.Models;

namespace SchoolHelpDeskAPI.Data
{
    public class SchoolHelpDeskContext : DbContext
    {
        public SchoolHelpDeskContext(DbContextOptions<SchoolHelpDeskContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketType> TicketTypes { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Map User.PasswordHash to DB column if needed
            modelBuilder.Entity<User>()
                .Property(u => u.Password_Hash)
                .HasColumnName("password_hash");

            modelBuilder.Entity<Comment>()
                .Property(c => c.TicketId)
                .HasColumnName("ticket_id");

            modelBuilder.Entity<Comment>()
                .Property(c => c.UserId)
                .HasColumnName("user_id");

            // Ticket mappings
            modelBuilder.Entity<Ticket>()
                .Property(t => t.UserId)
                .HasColumnName("user_id");

            modelBuilder.Entity<Ticket>()
                .Property(t => t.TypeId)
                .HasColumnName("type_id");
        }
    }
}
