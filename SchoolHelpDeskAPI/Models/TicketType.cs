using System.ComponentModel.DataAnnotations;

namespace SchoolHelpDeskAPI.Models
{
	public class TicketType
	{
		public int Id { get; set; }

		[Required]
		public string Name { get; set; }

		public string Description { get; set; }
	}
}
