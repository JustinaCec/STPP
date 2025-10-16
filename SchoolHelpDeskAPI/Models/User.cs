using System.ComponentModel.DataAnnotations;

namespace SchoolHelpDeskAPI.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password_Hash { get; set; }

        [Required]
        public string Role { get; set; } // "Student" or "Admin"
        public List<string>? RefreshTokens { get; set; }
        public List<DateTime>? RefreshTokenExpiries { get; set; }
    }
}
