using ASP_Server.Services;
using AttendanceJournalLibrary;
using Microsoft.Extensions.Caching.Memory;

namespace ASP_Server.CachedTables
{
    public class AttendancesCachService
    {
		private readonly ILoadingDB _loadDB;
		private IMemoryCache _cache;

		public AttendancesCachService(ILoadingDB loadDB, IMemoryCache cache)
		{
			_loadDB = loadDB;
			_cache = cache;

			foreach (var u in _loadDB.DB.Attendances)
			{
				_cache.Set(u.AttendanceID, u);
			}
		}

		public IEnumerable<Attendance> GetAttendances()
		{
			string cacheKey = "AllAttendances";
			if (!_cache.TryGetValue(cacheKey, out List<Attendance>? attendances))
			{
				Console.WriteLine("Данные Attendances взяты из БД");
				attendances = _loadDB.DB.Attendances.ToList();
				_cache.Set(cacheKey, attendances);
			}
			return attendances;
		}

		public async Task AddAttendance(Attendance attendance)
		{
            AttendanceJournalContext contex = _loadDB.DB;
			contex.Attendances.Add(attendance);
			int count = await contex.SaveChangesAsync();
			if (count > 0)
				_cache.Set($"attendance-{attendance.AttendanceID}", attendance);
		}

		public async Task UpdateAttendance(Attendance attendance)
		{
            AttendanceJournalContext contex = _loadDB.DB;
			contex.Attendances.Update(attendance);
			await contex.SaveChangesAsync();
			_cache.Set($"attendance-{attendance.AttendanceID}", attendance);
		}

		public async Task DeleteAttendance(Attendance attendance)
		{
            AttendanceJournalContext contex = _loadDB.DB;
			contex.Attendances.Remove(attendance);
			int count = await contex.SaveChangesAsync();
			if (count > 0)
				_cache.Remove($"attendance-{attendance.AttendanceID}");
		}

		public Attendance? GetAttendance(int id)
		{
			Attendance? attendance = null;
			if (!_cache.TryGetValue(id, out attendance))
			{
				attendance = _loadDB.DB.Attendances.FirstOrDefault(u => u.AttendanceID == id);
				if (attendance != null)
					_cache.Set($"attendance-{attendance.AttendanceID}", attendance);
			}
			return attendance;
		}
	}
}
