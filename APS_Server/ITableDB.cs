using AttendanceJournalLibrary;

namespace ASP_Server.Services
{
    public interface ILoadingDB
    {
        AttendanceJournalContext DB { get; }
    }
}
