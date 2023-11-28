using System.ComponentModel.DataAnnotations;

namespace AttendanceJournalLibrary
{
    public partial class Role
    {
        public int RoleID { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
    }
}
