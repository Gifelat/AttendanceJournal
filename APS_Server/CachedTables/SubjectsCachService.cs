using ASP_Server.Services;
using AttendanceJournalLibrary;
using Microsoft.Extensions.Caching.Memory;

namespace ASP_Server.CachedTables
{
    public class SubjectsCachService
    {
		private readonly ILoadingDB _loadDB;
		private IMemoryCache _cache;

        public SubjectsCachService(ILoadingDB loadDB, IMemoryCache cache)
        {
            _loadDB = loadDB;
            _cache = cache;

			foreach (var u in _loadDB.DB.Subjects)
			{
				_cache.Set(u.SubjectID, u);
			}
		}

		public IEnumerable<Subject>? GetSubjects()
		{
			string cacheKey = "AllSubject";
			if (!_cache.TryGetValue(cacheKey, out List<Subject>? subjects))
			{
				Console.WriteLine("Данные subjects взяты из БД");
				subjects = _loadDB.DB.Subjects.ToList();
				_cache.Set(cacheKey, subjects);
			}

			return subjects;
		}

		public async Task AddSubject(Subject subject)
		{
            AttendanceJournalContext contex = _loadDB.DB;
			contex.Subjects.Add(subject);
			int count = await contex.SaveChangesAsync();
			if (count > 0)
				_cache.Set($"Subject-{subject.SubjectID}", subject);
		}

		public async Task UpdateSubject(Subject subject)
		{
            AttendanceJournalContext contex = _loadDB.DB;
			contex.Subjects.Update(subject);
			await contex.SaveChangesAsync();
			_cache.Set($"Subject-{subject.SubjectID}", subject);
		}

		public async Task DeleteSubject(Subject subject)
		{
            AttendanceJournalContext contex = _loadDB.DB;
			contex.Subjects.Remove(subject);
			int count = await contex.SaveChangesAsync();
			if (count > 0)
				_cache.Remove($"Subject-{subject.SubjectID}");
		}

		public Subject? GetSubject(int id)
		{
			Subject? subject = null;
			if (!_cache.TryGetValue(id, out subject))
			{
				subject = _loadDB.DB.Subjects.FirstOrDefault(u => u.SubjectID == id);
				if (subject != null)
					_cache.Set($"Subject-{subject.SubjectID}", subject);
			}
			return subject;
		}
	}
}
