using Microsoft.AspNetCore.Mvc;
using ASP_Server.Services;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;
using ASP_Server.CachedTables;
using ASP_Server.Models;
using AttendanceJournalLibrary;

namespace ASP_Server.Controllers
{
	[Authorize]
	public class FacultiesController : Controller
	{
		private readonly ILoadingDB _loadDB;
		private FacultiesCachService _cache;

		public FacultiesController(ILoadingDB loadingDB, FacultiesCachService cache)
		{
			_loadDB = loadingDB;
			_cache = cache;
		}

		[Route("Faculties")]
		public IActionResult Main() => View();

		[ResponseCache(Location = ResponseCacheLocation.Any, Duration = 2 * 6 + 240)]
		public IActionResult All() => View("Output", _cache.GetFaculties());

		[HttpGet]
		public IActionResult Result(string id, string facultyname, string departmentname, string groupname) 
		{
			FacultySearchModel model = new FacultySearchModel()
			{
				FacultyID = id,
				FacultyName = facultyname,
				DepartmentName = departmentname,
				GroupName = groupname
			};
			return View("Output", GetSearch(model));
		}

		public IActionResult Search()
		{
			FacultySearchModel model = new FacultySearchModel()
			{
				FacultyID = HttpContext.Session.GetString("Faculty_id"),
				FacultyName = HttpContext.Session.GetString("Faculty_FacultyName"),
				DepartmentName = HttpContext.Session.GetString("Faculty_DepartmentName"),
				GroupName = HttpContext.Session.GetString("Faculty_GroupName")
			};

			return View(model);
		}

		public IActionResult Add() => View();

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddFaculty(Faculty faculty)
		{
			if (ModelState.IsValid)
			{
				await _cache.AddFaculty(faculty);
				return View("Main");
			}
			else
				return View("Add");
		}


		[HttpGet]
		public IActionResult Edit(int id)
		{
			Faculty? faculty = _cache.GetFaculty(id);
			if (faculty != null)
				return View(faculty);
			else
				return RedirectToAction("ErrorPage", "Home");
		}

		[HttpPut]
		public async Task<IActionResult> EditFaculty([FromBody] Faculty faculty)
		{
			if (!IsTypeName(faculty))
				return Json(new { typeNameError = "Факультет уже существует" });

			if (ModelState.IsValid)
			{
				 await _cache.UpdateFaculty(faculty);
				return Json(new { success = true });
			}
			else
				return Json(new { success = false });
		}

		[HttpGet]
		public IActionResult Delete(int id)
		{
			Faculty? faculty = _cache.GetFaculty(id);
			ViewData["Message"] = (faculty == null) ? $"Запись №{id} удалена!" : "Запись не удалена или не найдена";
			return View();
		}

		[HttpDelete]
		public async Task<IActionResult> DeleteFaculty(int id)
		{
			try
			{
				Faculty? faculty = _cache.GetFaculty(id);
				await _cache.DeleteFaculty(faculty);
				return Json(new { success = true });
			}
			catch
			{
				return Json(new { success = false });
			}
		}

		[NonAction]
		private List<Faculty> GetSearch(FacultySearchModel model)
		{
			HttpContext.Session.SetString("Faculty_id", (model.FacultyID != null) ? model.FacultyID : "");
			HttpContext.Session.SetString("Faculty_FacultyName", (model.FacultyName != null) ? model.FacultyName : "");
			HttpContext.Session.SetString("Faculty_DepartmentName", (model.DepartmentName != null) ? model.DepartmentName : "");
			HttpContext.Session.SetString("Faculty_GroupName", (model.GroupName != null) ? model.GroupName : "");

			List<Faculty> faculties = new List<Faculty>();
			List<Faculty> departmentname = new List<Faculty>();
			List<Faculty> groupname = new List<Faculty>();
			List<Faculty>? filter = null;

			if (model.FacultyID == null && model.FacultyName == null)
				faculties = _cache.GetFaculties().ToList();
			else
			{
				if (model.FacultyID != null)
				{
					filter = _cache.GetFaculties().ToList().Where(u => Regex.IsMatch(u.FacultyID.ToString(), model.FacultyID)).ToList();

					if (filter != null)
						faculties.AddRange(filter);
				}

				if(model.FacultyName != null )
				{
					if (faculties.Count != 0)
					{
						filter = faculties.Where(u => Regex.IsMatch(u.FacultyName, model.FacultyName)).ToList();
						faculties.Clear();
					}
					else
						filter = _cache.GetFaculties().ToList().Where(u => Regex.IsMatch(u.FacultyName, model.FacultyName)).ToList();
					
					if (filter != null)
						faculties.AddRange(filter);
				}
			}
			return faculties;
		}

		[NonAction]
		private bool IsTypeName(Faculty faculty)
		{
			Faculty? check = _cache.GetFaculties().FirstOrDefault(u => u.FacultyName == faculty.FacultyName && u.FacultyID != faculty.FacultyID);

			if (check != null)
				return false;

			return true;
		}
	}
}
