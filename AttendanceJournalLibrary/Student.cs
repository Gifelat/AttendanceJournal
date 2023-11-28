namespace AttendanceJournalLibrary;

public partial class Student
{
    public int StudentID { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public int FacultyID { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual Faculty Faculty { get; set; } = null!;
}
