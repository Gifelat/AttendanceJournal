using ASP_Server.Services;
using AttendanceJournalLibrary;
using Microsoft.Extensions.Caching.Memory;

namespace ASP_Server.CachedTables
{
	public class TeachersCachService
	{
		private readonly ILoadingDB _loadDB;
		private readonly IMemoryCache _cache;

		public TeachersCachService(ILoadingDB loadDB, IMemoryCache memoryCache)
		{
			_loadDB = loadDB;
			_cache = memoryCache;

			foreach(var u in _loadDB.DB.Teachers)
			{
				_cache.Set(u.TeacherID, u);
			}
		}

		public IEnumerable<Teacher>? GetTeachers()
		{
			string cacheKey = "AllTeachers";
			if (!_cache.TryGetValue(cacheKey, out List<Teacher>? teachers))
			{
				Console.WriteLine("Данные Teachers взяты из БД");
				teachers = _loadDB.DB.Teachers.ToList();
				_cache.Set(cacheKey, teachers);
			}

			return teachers;
		}

		public async Task AddTeacher(Teacher teacher)
		{
            AttendanceJournalLibrary.AttendanceJournalContext contex = _loadDB.DB;
			contex.Teachers.Add(teacher);
			int count = await contex.SaveChangesAsync();
			if (count > 0)
				_cache.Set($"teacher-{teacher.TeacherID}", teacher);
		}

		public async Task UpdateTeacher(Teacher teacher)
		{
            AttendanceJournalLibrary.AttendanceJournalContext contex = _loadDB.DB;
			contex.Teachers.Update(teacher);
			await contex.SaveChangesAsync();
			_cache.Set($"teacher-{teacher.TeacherID}", teacher);
		}

		public async Task DeleteTeacher(Teacher teacher)
		{
            AttendanceJournalLibrary.AttendanceJournalContext contex = _loadDB.DB;
			contex.Teachers.Remove(teacher);
			int count = await contex.SaveChangesAsync();
			if (count > 0)
				_cache.Remove($"teacher-{teacher.TeacherID}");
		}

		public Teacher? GetTeacher(int id)
		{
			Teacher? teacher = null;
			if (!_cache.TryGetValue(id, out teacher))
			{
				teacher = _loadDB.DB.Teachers.FirstOrDefault(u => u.TeacherID == id);
				if (teacher != null)
					_cache.Set($"teacher-{teacher.TeacherID}", teacher);
			}
			return teacher;
		}
	}
}
