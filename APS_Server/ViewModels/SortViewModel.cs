namespace ASP_Server_MVC.ViewModels
{
	public enum SortState
	{
		No,
		AttendanceIDAsc,
		AttendanceIDDesc,
		ClasseNameAsc,
		ClasseNameDesc
	}
	public class SortViewModel
	{
		public SortState AttendanceIDSort { get; set; }
		public SortState ClasseName { get; set; }
		public SortState CurrentState { get; set; }

		public SortViewModel(SortState sortOrder)
		{
			AttendanceIDSort = sortOrder == SortState.AttendanceIDAsc ? SortState.AttendanceIDDesc : SortState.AttendanceIDAsc;
			ClasseName = sortOrder == SortState.ClasseNameAsc ? SortState.ClasseNameDesc : SortState.ClasseNameAsc;
			CurrentState = sortOrder;
		}


	}
}
