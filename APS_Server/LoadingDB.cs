using AttendanceJournalLibrary;

namespace ASP_Server.Services
{
    public class LoadingDB : ILoadingDB
    {
        public AttendanceJournalContext DB => new AttendanceJournalContext();
    }
}
