namespace AttendanceJournalLibrary;

public partial class Attendance
{
    public int AttendanceID { get; set; }

    public int ClassID { get; set; }

    public int StudentID { get; set; }

    public bool IsPresent { get; set; }

	public int TeacherID { get; set; }

	public virtual Class Class { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;

	public virtual Teacher Teacher { get; set; } = null!;
}
