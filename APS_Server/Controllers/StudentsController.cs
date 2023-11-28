using ASP_Server.CachedTables;
using ASP_Server.Services;
using ASP_Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Text.RegularExpressions;
using AttendanceJournalLibrary;

namespace ASP_Server.Controllers
{
	[Authorize]
	public class StudentsController : Controller
	{
		private readonly int pageSize = 50;
		private readonly ILoadingDB _loadDB;
		private StudentsCachService _cache;

		public StudentsController(ILoadingDB loadingDB, StudentsCachService cache)
		{
			_loadDB = loadingDB;
			_cache = cache;
		}

		[Route("Students")]
		public IActionResult Main() => View();

		[ResponseCache(Location = ResponseCacheLocation.Any, Duration = 2 * 6 + 240)]
		public IActionResult All(bool full)
		{
			if (User.FindFirst(x => x.Type == System.Security.Claims.ClaimsIdentity.DefaultRoleClaimType).Value != "Admin")
				full = false;

			return View("Output", GetStudents(full));
		}

		[HttpGet]
		public IActionResult Result(string id, string lastname, string firstname, string student, string faculty)
		{
			StudentSearchModel model = new StudentSearchModel()
			{
				StudentID = id,
				FirstName = firstname,
				LastName = lastname,
				Faculty = faculty,
			};

			return View("Output", GetSearch(model, false));
		}

		[HttpGet]
		public IActionResult Search()
		{
			StudentSearchModel model = new StudentSearchModel()
			{
				StudentID = HttpContext.Session.GetString("Student_id"),
				LastName = HttpContext.Session.GetString("Student_LastName"),
				FirstName = HttpContext.Session.GetString("Student_FirstName"),
				Faculty = HttpContext.Session.GetString("Student_faculty")
			};
			return View(model);
		}

		public IActionResult Add()
		{
			ViewBag.Faculties = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_loadDB.DB.Faculties.ToList(), "FacultyID", "FacultyName");
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddStudent(Student student)
		{
			ViewBag.Faculties = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_loadDB.DB.Faculties.ToList(), "FacultyID", "FacultyName");

			AttendanceJournalContext context = _loadDB.DB;

			if (ModelState.IsValid)
			{
				await _cache.AddStudent(student);
				return View("Main");
			}
			else
				return View("Add");
		}

		[HttpGet]
		public IActionResult Edit(int id)
		{
			ViewBag.Faculties = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_loadDB.DB.Faculties.ToList(), "FacultyID", "FacultyName");
			Student? student = _cache.GetStudent(id);
			if (student != null)
				return View(student);
			else
				return RedirectToAction("ErrorPage", "Home");
		}

		[HttpGet]
		public IActionResult Delete(int id)
		{
			Student? student = GetStudents(true).FirstOrDefault(u => u.StudentID == id);
			ViewData["Message"] = (student == null) ? $"Запись №{id} удалена!" : "Запись не удалена или не найдена";
			return View();
		}

		[HttpDelete]
		public async Task<IActionResult> DeleteStudent(int id)
		{
			try
			{
				Student? student = _cache.GetStudent(id);
				await _cache.DeleteStudent(student);
				return Json(new { success = true });
			}
			catch
			{
				return Json(new { success = false });
			}
		}

		[NonAction]
		private List<Student> GetStudents(bool full)
		{
			Teacher? teacher = _loadDB.DB.Teachers.FirstOrDefault(u => u.Login == User.Identity.Name);
			List<Student> students = new List<Student>();
			List<Faculty> faculties = _loadDB.DB.Faculties.ToList();
			List<Class> classes = _loadDB.DB.Classes.ToList();
			List<Subject> Subjects = _loadDB.DB.Subjects.ToList();

			foreach (var classa in classes)
			{
				classa.Subject = Subjects.FirstOrDefault(u => u.SubjectID == classa.SubjectID);
			}

			/*foreach (var student in (full) ? _loadDB.DB.Students.ToList() : _loadDB.DB.Students.Where(u => u.StudentID == teacher.FacultyID).ToList())
			{
				student.Faculty = (full) ? faculties.FirstOrDefault(u => u.FacultyID == student.FacultyID) : faculty;
				student.LastName = lastnames.FirstOrDefault(u => u.LastNameID == student.LastNameID);
				*//*student.Student = faculties.FirstOrDefault(u => u.StudentID == student.StudentID);*//*
				students.Add(student);
			}*/

			return students;
		}

		[NonAction]
		private List<Student> GetSearch(StudentSearchModel model, bool full)
		{
			HttpContext.Session.SetString("Student_id", (model.StudentID != null) ? model.StudentID : "");
			HttpContext.Session.SetString("Student_LastName", (model.LastName != null) ? model.LastName : "");
			HttpContext.Session.SetString("Student_FirstName", (model.FirstName != null) ? model.FirstName : "");
			HttpContext.Session.SetString("Student_faculty", (model.Faculty != null) ? model.Faculty : "");

			List<Student> studentData = GetStudents(full);
			List<Student> students = new List<Student>();
			List<Student>? filter = null;

			if (model.StudentID == null && model.LastName == null && model.FirstName == null && model.Faculty == null)
				students = studentData;
			else
			{
				if (model.StudentID != null)
				{
					filter = studentData.Where(u => Regex.IsMatch(u.StudentID.ToString(), model.StudentID)).ToList();

					if (filter != null)
						students.AddRange(filter);
				}

				if (model.LastName != null)
				{
					if (students.Count != 0)
					{
						filter = students.Where(u => Regex.IsMatch(u.LastName.ToString(), model.LastName)).ToList();
						students.Clear();
					}
					else
						filter = studentData.Where(u => Regex.IsMatch(u.LastName.ToString(), model.LastName)).ToList();

					if (filter != null)
						students.AddRange(filter);
				}

				if (model.FirstName != null)
				{
					if (students.Count != 0)
					{
						filter = students.Where(u => Regex.IsMatch(u.FirstName.ToString(), model.FirstName)).ToList();
						students.Clear();
					}
					else
						filter = studentData.Where(u => Regex.IsMatch(u.FirstName.ToString(), model.FirstName)).ToList();

					if (filter != null)
						students.AddRange(filter);
				}

				if (model.Faculty != null)
				{
					if (students.Count != 0)
					{
						filter = students.Where(u => Regex.IsMatch(u.Faculty.ToString(), model.Faculty)).ToList();
						students.Clear();
					}
					else
						filter = studentData.Where(u => Regex.IsMatch(u.Faculty.ToString(), model.Faculty)).ToList();

					if (filter != null)
						students.AddRange(filter);
				}
			}
			return students;
		}

	}
}
