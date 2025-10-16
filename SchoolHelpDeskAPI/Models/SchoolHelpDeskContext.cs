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
        public DbSet<RefreshToken> RefreshTokens { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Existing mappings
            modelBuilder.Entity<User>()
                .Property(u => u.Password_Hash)
                .HasColumnName("password_hash");

            modelBuilder.Entity<Comment>()
                .Property(c => c.TicketId)
                .HasColumnName("ticket_id");

            modelBuilder.Entity<Comment>()
                .Property(c => c.UserId)
                .HasColumnName("user_id");

            modelBuilder.Entity<Ticket>()
                .Property(t => t.UserId)
                .HasColumnName("user_id");

            modelBuilder.Entity<Ticket>()
                .Property(t => t.TypeId)
                .HasColumnName("type_id");

            // RefreshToken mappings
            modelBuilder.Entity<RefreshToken>()
                .Property(rt => rt.Expiry)
                .HasColumnType("timestamp with time zone");

            modelBuilder.Entity<RefreshToken>()
                .Property(rt => rt.CreatedAt)
                .HasColumnType("timestamp with time zone");

            modelBuilder.Entity<RefreshToken>()
                .Property(rt => rt.RevokedAt)
                .HasColumnType("timestamp with time zone");

            modelBuilder.Entity<User>()
                .HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
