using System;
using System.ComponentModel.DataAnnotations;

namespace SchoolHelpDeskAPI.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public int TicketId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string Body { get; set; }
    }
}
