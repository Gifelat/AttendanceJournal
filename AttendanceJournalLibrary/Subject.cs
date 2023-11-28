namespace AttendanceJournalLibrary;

public partial class Subject
{
    public int SubjectID { get; set; }

    public string SubjectName { get; set; } = null!;

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

}
