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
	public class SubjectsController : Controller
    {
        private readonly ILoadingDB _loadDB;
		private SubjectsCachService _cache;

		public SubjectsController(ILoadingDB loadingDB, SubjectsCachService cache)
        {
            _loadDB = loadingDB;
			_cache = cache;
		}

        [Route("Subjects")]
		public IActionResult Main() => View();

		[ResponseCache(Location = ResponseCacheLocation.Any, Duration = 2 * 6 + 240)]
		public IActionResult All() => View("Output", _cache.GetSubjects());

		[HttpGet]
		public IActionResult Result(string id, string subject)
		{
			SubjectsearchModel model = new SubjectsearchModel()
			{
				SubjectID = id,
                SubjectName = subject
            };
			return View("Output", GetSearch(model)); 
		}

		public IActionResult Search() 
		{
			SubjectsearchModel model = new SubjectsearchModel()
			{
				SubjectID = HttpContext.Session.GetString("Subject_id"),
                SubjectName = HttpContext.Session.GetString("Subject_Name")
			};

			return View(model);
		}

		public IActionResult Add() => View();

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddSubject(Subject subject)
        {
            if (ModelState.IsValid)
			{
				await _cache.AddSubject(subject);
				return View("Main");
			}
            else
				return View("Add");
		}

        [HttpGet]
        public IActionResult Edit(int id)
        {
			Subject? Subject = _cache.GetSubject(id);
			if (Subject != null)
				return View(Subject);
			else
				return RedirectToAction("ErrorPage", "Home");
        }

        [HttpPut]
        public async Task<IActionResult> EditSubject([FromBody] Subject Subject)
		{
			if (!IsSubjectName(Subject))
				return Json(new { NameError = "Предмет уже существует" });

			if (ModelState.IsValid)
			{
				await _cache.UpdateSubject(Subject);
				return Json(new { success = true });
			}
			else
				return Json(new { success = false });
		}

		[HttpGet]
        public IActionResult Delete(int id)
        {
			Subject? Subject = _cache.GetSubject(id);
			ViewData["Message"] = (Subject == null) ? $"Запись №{id} удалена!" : "Запись не удалена или не найдена";
			return View();
		}

		[HttpDelete]
		public async Task<IActionResult> DeleteSubject(int id)
		{
			try
			{
				Subject? Subject = _cache.GetSubject(id);
				await _cache.DeleteSubject(Subject);
				return Json(new { success = true });
			}
			catch
			{
				return Json(new { success = false });
			}
		}

		[NonAction]
		private List<Subject> GetSearch(SubjectsearchModel model)
		{
			HttpContext.Session.SetString("Subject_id", (model.SubjectID != null) ? model.SubjectID : "");
			HttpContext.Session.SetString("Subject_Name", (model.SubjectName != null) ? model.SubjectName : "");

			List<Subject> Subjects = new List<Subject>();
			List<Subject>? filter = null;

			if (model.SubjectID == null && model.SubjectName == null)
				Subjects = _cache.GetSubjects().ToList();
			else
			{
				if (model.SubjectID != null)
				{
					filter = _cache.GetSubjects().ToList().Where(u => Regex.IsMatch(u.SubjectID.ToString(), model.SubjectID)).ToList();

					if (filter != null)
						Subjects.AddRange(filter);
				}

				if (model.SubjectName != null)
				{
					if (Subjects.Count != 0)
					{
						filter = Subjects.Where(u => Regex.IsMatch(u.SubjectName, model.SubjectName)).ToList();
						Subjects.Clear();
					}
					else
						filter = _cache.GetSubjects().ToList().Where(u => Regex.IsMatch(u.SubjectName, model.SubjectName)).ToList();

					if (filter != null)
						Subjects.AddRange(filter);
				}
			}
			return Subjects;
		}

		[NonAction]
		private bool IsSubjectName(Subject Subject)
		{
			Subject? check = _cache.GetSubjects().FirstOrDefault(u => u.SubjectName == Subject.SubjectName && u.SubjectID != Subject.SubjectID);

			if (check != null)
				return false;

			return true;
		}
	}
}
