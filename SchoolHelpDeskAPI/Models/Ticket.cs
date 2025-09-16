using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchoolHelpDeskAPI.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public int? TypeId { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public string Status { get; set; } // "Open", "Pending", "Closed"

        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}
