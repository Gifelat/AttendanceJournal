using ASP_Server.Services;
using AttendanceJournalLibrary;
using Microsoft.Extensions.Caching.Memory;

namespace ASP_Server.CachedTables
{
	public class StudentsCachService
	{
		private readonly ILoadingDB _loadDB;
		private IMemoryCache _cache;

		public StudentsCachService(ILoadingDB loadDB, IMemoryCache cache)
		{
			_loadDB = loadDB;
			_cache = cache;

			foreach (var u in _loadDB.DB.Students)
			{
				_cache.Set(u.StudentID, u);
			}
		}

		public IEnumerable<Student>? GetStudents()
		{
			string cacheKey = "AllSubject";
			if (!_cache.TryGetValue(cacheKey, out List<Student>? classes))
			{
				Console.WriteLine("Данные Students взяты из БД");
				classes = _loadDB.DB.Students.ToList();
				_cache.Set(cacheKey, classes);
			}

			return classes;
		}

		public async Task AddStudent(Student classa)
		{
			AttendanceJournalContext contex = _loadDB.DB;
			contex.Students.Add(classa);
			int count = await contex.SaveChangesAsync();
			if (count > 0)
				_cache.Set($"classa-{classa.StudentID}", classa);
		}

		public async Task UpdateStudent(Student classa)
		{
			AttendanceJournalContext contex = _loadDB.DB;
			contex.Students.Update(classa);
			await contex.SaveChangesAsync();
			_cache.Set($"classa-{classa.StudentID}", classa);
		}

		public async Task DeleteStudent(Student classa)
		{
			AttendanceJournalContext contex = _loadDB.DB;
			contex.Students.Remove(classa);
			int count = await contex.SaveChangesAsync();
			if (count > 0)
				_cache.Remove($"classa-{classa.StudentID}");
		}

		public Student? GetStudent(int id)
		{
			Student? classa = null;
			if (!_cache.TryGetValue(id, out classa))
			{
				classa = _loadDB.DB.Students.FirstOrDefault(u => u.StudentID == id);
				if (classa != null)
					_cache.Set($"classa-{classa.StudentID}", classa);
			}
			return classa;
		}
	}
}
