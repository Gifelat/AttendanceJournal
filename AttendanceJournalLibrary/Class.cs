namespace AttendanceJournalLibrary;

public partial class Class
{
    public int ClassID { get; set; }

    public int SubjectID { get; set; }

    public int FacultyID { get; set; }

    public string Theme { get; set; } = null!;

    public DateTime ClassDate { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual Subject Subject { get; set; } = null!;

    public virtual Faculty Facultie { get; set; } = null!;
}
