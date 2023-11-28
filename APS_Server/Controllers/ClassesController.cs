using ASP_Server.CachedTables;
using ASP_Server.Services;
using ASP_Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using AttendanceJournalLibrary;

namespace ASP_Server.Controllers
{
	[Authorize]
	public class ClassesController : Controller
	{
		private readonly ILoadingDB _loadDB;
		private ClassesCachService _cache;

		public ClassesController(ILoadingDB loadingDB, ClassesCachService cache)
		{
			_loadDB = loadingDB;
			_cache = cache;
		}

		[Route("Classes")]
		public IActionResult Main() => View();

		[ResponseCache(Location = ResponseCacheLocation.Any, Duration = 2 * 6 + 240)]
		public IActionResult All() => View("Output", GetClasses());

		[HttpGet]
		public IActionResult Result(string id, string theme, string faculties, string typeclassa)
		{
			ClassSearchModel model = new ClassSearchModel()
			{
				ClassID = id,
				Theme = theme,
				Faculty = faculties,
				Subject = typeclassa
			};
			return View("Output", GetSearch(model));
		}

		public IActionResult Search()
		{
			ClassSearchModel model = new ClassSearchModel()
			{
				ClassID = HttpContext.Session.GetString("Classe_id"),
				Theme = HttpContext.Session.GetString("Classe_theme"),
				Subject = HttpContext.Session.GetString("Classe_type")
			};

			return View(model);
		}

		public IActionResult Add()
		{
			ViewBag.Subjects = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_loadDB.DB.Subjects.ToList(), "SubjectId", "TypeTheme");
			return View();
		}

		[HttpGet]
		public IActionResult Edit(int id)
		{
			Class? classa = _cache.GetClasses(id);
			if (classa != null)
			{
				ViewBag.Subjects = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_loadDB.DB.Subjects.ToList(), "SubjectId", "TypeTheme");
				return View(classa);
			}
			else
				return RedirectToAction("ErrorPage", "Home");
		}

		[HttpPut]
		public async Task<IActionResult> EditClasse(bool save, IFormFile image, [FromForm] Class classa)
		{
			if (!IsTheme(classa))
				return Json(new { themeError = "Уже существует" });

			FileInfo[] list = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img")).GetFiles();
			string imgTheme = "";

			if (!save && image != null && image.Length > 0)
			{
				imgTheme = (list.Length + 1) + ".jpg";
				string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", imgTheme);
				using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
				{
					image.CopyTo(fileStream);
				}
			}

		/*	if (save)
				imgTheme = _cache.GetClasses().FirstOrDefault(u => u.ClassID == classa.ClassID).Image;*/

			try
			{
				/*classa.Image = (imgTheme != null) ? imgTheme : "";*/
				await _cache.UpdateClasses(classa);
				return Json(new { success = true });
			}
			catch
			{
				return Json(new { success = false });
			}
		}

		[HttpGet]
		public IActionResult Delete(int id)
		{
			Class? classa = _cache.GetClasses(id);
			ViewData["Message"] = (classa == null) ? $"Запись №{id} удалена!" : "Запись не удалена или не найдена";
			return View();
		}

		[HttpDelete]
		public async Task<IActionResult> DeleteClasse(int id)
		{
			try
			{
				Class? classa = _cache.GetClasses(id);
				await _cache.DeleteClasses(classa);
				return Json(new { success = true });
			}
			catch
			{
				return Json(new { success = false });
			}
		}

		[NonAction]
		private List<Class> GetClasses()
		{
			List<Class> classes = new List<Class>();
			List<Subject> type = _loadDB.DB.Subjects.ToList();
			foreach (var classa in _cache.GetClasses())
			{
				/*if (classa.Image == null)
					classa.Image = "";

				classa.Subject = type.FirstOrDefault(u => u.SubjectID == classa.SubjectID);*/
				classes.Add(classa);
			}

			return classes;
		}

		[NonAction]
		private List<Class> GetSearch(ClassSearchModel model)
		{
			HttpContext.Session.SetString("Classe_id", (model.ClassID != null) ? model.ClassID : "");
			HttpContext.Session.SetString("Classe_theme", (model.Theme != null) ? model.Theme : "");
			HttpContext.Session.SetString("Classe_type", (model.Subject != null) ? model.Subject : "");

			List<Class> classes = new List<Class>();
			List<Class> filter = null;

			if (model.ClassID == null && model.Theme == null && model.Faculty == null && model.Subject == null)
				classes = GetClasses();
			else
			{
				if (model.ClassID != null)
				{
					filter = GetClasses().Where(u => Regex.IsMatch(u.ClassID.ToString(), model.ClassID)).ToList();

					if (filter != null)
						classes.AddRange(filter);
				}

				if (model.Theme != null)
				{
					/*if (classes.Count != 0)
					{
						filter = classes.Where(u => Regex.IsMatch(u.Theme, model.Theme)).ToList();
						classes.Clear();
					}
					else
						filter = GetClasses().Where(u => Regex.IsMatch(u.Theme, model.Theme)).ToList();*/

					if (filter != null)
						classes.AddRange(filter);
				}

			}
			return classes;
		}

		[NonAction]
		private bool IsTheme(Class classa)
		{
			/*Class? check = _cache.GetClasses().FirstOrDefault(u => u.Theme == classa.Theme && u.ClassID != classa.ClassID);

			if (check != null)
				return false;*/

			return true;
		}
	}
}
