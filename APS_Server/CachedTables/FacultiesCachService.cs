using ASP_Server.Services;
using AttendanceJournalLibrary;
using Microsoft.Extensions.Caching.Memory;

namespace ASP_Server.CachedTables
{
    public class FacultiesCachService
    {
		private readonly ILoadingDB _loadDB;
		private IMemoryCache _cache;

		public FacultiesCachService(ILoadingDB loadDB, IMemoryCache cache)
		{
			_loadDB = loadDB;
			_cache = cache;

			foreach (var u in _loadDB.DB.Faculties)
			{
				_cache.Set(u.FacultyID, u);
			}
		}

		public IEnumerable<Faculty>? GetFaculties()
		{
			string cacheKey = "AllStudent";
			if (!_cache.TryGetValue(cacheKey, out List<Faculty>? faculties))
			{
				Console.WriteLine("Данные Faculties взяты из БД");
				faculties = _loadDB.DB.Faculties.ToList();
				_cache.Set(cacheKey, faculties);
			}
			return faculties;
		}

		public async Task AddFaculty(Faculty faculty)
		{
            AttendanceJournalContext contex = _loadDB.DB;
			contex.Faculties.Add(faculty);
			int count = await contex.SaveChangesAsync();
			if (count > 0)
				_cache.Set($"faculty-{faculty.FacultyID}", faculty);
		}

		public async Task UpdateFaculty(Faculty faculty)
		{
            AttendanceJournalContext contex = _loadDB.DB;
			contex.Faculties.Update(faculty);
			await contex.SaveChangesAsync();
			_cache.Set($"faculty-{faculty.FacultyID}", faculty);
		}

		public async Task DeleteFaculty(Faculty faculty)
		{
            AttendanceJournalContext contex = _loadDB.DB;
			contex.Faculties.Remove(faculty);
			int count = await contex.SaveChangesAsync();
			if (count > 0)
				_cache.Remove($"faculty-{faculty.FacultyID}");
		}

		public Faculty? GetFaculty(int id)
		{
			Faculty? faculty = null;
			if (!_cache.TryGetValue(id, out faculty))
			{
				faculty = _loadDB.DB.Faculties.FirstOrDefault(u => u.FacultyID == id);
				if (faculty != null)
					_cache.Set($"faculty-{faculty.FacultyID}", faculty);
			}
			return faculty;
		}
	}
}
