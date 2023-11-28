using System.ComponentModel.DataAnnotations;

namespace ASP_Server_MVC.ViewModels
{
	public class AttendanceViewModel
	{
		public int AttendanceID { get; set; }

		public string Classe { get; set; }

		public bool IsPresent { get; set; }

		[DataType(DataType.Date)]
		public DateTime Date { get; set; }
		public TimeSpan Time { get; set; }
		public string Student { get; set; }
		public string Teacher { get; set; }
		public SortViewModel SortViewModel { get; set; }
	}
}
