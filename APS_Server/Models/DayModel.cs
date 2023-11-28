using System.ComponentModel.DataAnnotations;

namespace ASP_Server.Models
{
	public class DayModel
	{
		public int Id { get; set; }
		[DataType(DataType.Date)]
		public DateTime Date { get; set; }
	}
}
