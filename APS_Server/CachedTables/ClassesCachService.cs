using ASP_Server.Services;
using AttendanceJournalLibrary;
using Microsoft.Extensions.Caching.Memory;

namespace ASP_Server.CachedTables
{
    public class ClassesCachService
    {
		private readonly ILoadingDB _loadDB;
		private IMemoryCache _cache;

		public ClassesCachService(ILoadingDB loadDB, IMemoryCache cache)
		{
			_loadDB = loadDB;
			_cache = cache;

			foreach (var u in _loadDB.DB.Classes)
			{
				_cache.Set(u.ClassID, u);
			}
		}

		public IEnumerable<Class>? GetClasses()
		{
			string cacheKey = "AllSubject";
			if (!_cache.TryGetValue(cacheKey, out List<Class>? classes))
			{
				Console.WriteLine("Данные Classes взяты из БД");
				classes = _loadDB.DB.Classes.ToList();
				_cache.Set(cacheKey, classes);
			}

			return classes;
		}

		public async Task AddClasses(Class classa)
		{
            AttendanceJournalContext contex = _loadDB.DB;
			contex.Classes.Add(classa);
			int count = await contex.SaveChangesAsync();
			if (count > 0)
				_cache.Set($"classa-{classa.ClassID}", classa);
		}

		public async Task UpdateClasses(Class classa)
		{
            AttendanceJournalContext contex = _loadDB.DB;
			contex.Classes.Update(classa);
			await contex.SaveChangesAsync();
			_cache.Set($"classa-{classa.ClassID}", classa);
		}

		public async Task DeleteClasses(Class classa)
		{
            AttendanceJournalContext contex = _loadDB.DB;
			contex.Classes.Remove(classa);
			int count = await contex.SaveChangesAsync();
			if (count > 0)
				_cache.Remove($"classa-{classa.ClassID}");
		}

		public Class? GetClasses(int id)
		{
			Class? classa = null;
			if (!_cache.TryGetValue(id, out classa))
			{
				classa = _loadDB.DB.Classes.FirstOrDefault(u => u.ClassID == id);
				if (classa != null)
					_cache.Set($"classa-{classa.ClassID}", classa);
			}
			return classa;
		}
	}
}
