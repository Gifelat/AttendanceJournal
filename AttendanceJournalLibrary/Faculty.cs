namespace AttendanceJournalLibrary;

public partial class Faculty
{
    public int FacultyID { get; set; }

    public string FacultyName { get; set; } = null!;

    public string DepartmentName { get; set; } = null!;

    public string GroupName { get; set; } = null!;

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    public virtual ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
