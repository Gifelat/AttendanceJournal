namespace AttendanceJournalLibrary;

public partial class Teacher
{
    public int TeacherID { get; set; }

    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string MiddleName { get; set; } = null!;

    public string Position { get; set; } = null!;

    public int FacultyID { get; set; }

    public int RoleID { get; set; }

	public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

	public virtual Faculty Faculty { get; set; } = null!;
    public virtual Role? Role { get; set; } = null!;
}
