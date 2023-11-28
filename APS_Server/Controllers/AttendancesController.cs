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
	public class AttendancesController : Controller
	{
		private readonly int pageSize = 50;
		private readonly ILoadingDB _loadDB;
		private AttendancesCachService _cache;

		public AttendancesController(ILoadingDB loadingDB, AttendancesCachService cache)
		{
			_loadDB = loadingDB;
			_cache = cache;
		}

		[Route("Attendances")]
		public IActionResult Main() => View();

		[ResponseCache(Location = ResponseCacheLocation.Any, Duration = 2 * 6 + 240)]
		public IActionResult All(bool full)
		{
			if (User.FindFirst(x => x.Type == System.Security.Claims.ClaimsIdentity.DefaultRoleClaimType).Value != "Admin")
				full = false;

			return View("Output", GetAttendances(full));
		}
		
		[HttpGet]
		public IActionResult Result(string id, string classe, string isPresent, string student, string teacher)
		{
			AttendanceSearchModel model = new AttendanceSearchModel()
			{
				AttendanceID = id,
				Class = classe,
				IsPresent = isPresent,
				Student = student,
				Teacher = teacher,
			};

			return View("Output", GetSearch(model, false));
		}

		[HttpGet]
		public IActionResult ResultDay(string isPresent) => View("Output", GetAttendances(false).Where(u => u.IsPresent.ToString() == isPresent).ToArray());
		public IActionResult Search()
		{
			AttendanceSearchModel model = new AttendanceSearchModel()
			{
				AttendanceID = HttpContext.Session.GetString("Attendance_id"),
				Class = HttpContext.Session.GetString("Attendance_classa"),
				IsPresent = HttpContext.Session.GetString("Attendance_isPresent"),
				Student = HttpContext.Session.GetString("Attendance_type"),
				Teacher = HttpContext.Session.GetString("Attendance_teacher")
			};
			return View(model);
		}

		public IActionResult Add()
		{
			ViewBag.Classes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_loadDB.DB.Classes.ToList(), "ClassID", "Name");
			ViewBag.Students = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_loadDB.DB.Faculties.ToList(), "FacultyId", "TypeName");
			ViewBag.Teachers = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_loadDB.DB.Teachers.ToList(), "TeacherID", "Login");
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddAttendance(Attendance attendance)
		{
			ViewBag.Classes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_loadDB.DB.Classes.ToList(), "ClassID", "Name");
			ViewBag.Students = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_loadDB.DB.Faculties.ToList(), "FacultyId", "TypeName");
			ViewBag.Teachers = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_loadDB.DB.Teachers.ToList(), "TeacherID", "Login");

			AttendanceJournalLibrary.AttendanceJournalContext context = _loadDB.DB;

			if (attendance.TeacherID == 0)
				attendance.TeacherID = _loadDB.DB.Teachers.FirstOrDefault(u => u.Login == User.Identity.Name).TeacherID;

			if (ModelState.IsValid)
			{
				await _cache.AddAttendance(attendance);
				return View("Main");
			}
			else
				return View("Add");
		}

		[HttpGet]
		public IActionResult Edit(int id)
		{
			ViewBag.Classes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_loadDB.DB.Classes.ToList(), "ClassID", "Name");
			ViewBag.Faculties = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_loadDB.DB.Faculties.ToList(), "FacultyId", "TypeName");
			ViewBag.Teachers = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_loadDB.DB.Teachers.ToList(), "TeacherID", "Login");
			Attendance? attendance = _cache.GetAttendance(id);
			if (attendance != null)
				return View(attendance);
			else
				return RedirectToAction("ErrorPage", "Home");
		}

		[HttpGet]
		public IActionResult Delete(int id)
		{
			Attendance? attendance = GetAttendances(true).FirstOrDefault(u => u.AttendanceID == id);
			ViewData["Message"] = (attendance == null) ? $"Запись №{id} удалена!" : "Запись не удалена или не найдена";
			return View();
		}

		[HttpDelete]
		public async Task<IActionResult> DeleteAttendance(int id)
		{
			try
			{
				Attendance? attendance = _cache.GetAttendance(id);
				await _cache.DeleteAttendance(attendance);
				return Json(new { success = true });
			}
			catch
			{
				return Json(new { success = false });
			}
		}

		[NonAction]
		private List<Attendance> GetAttendances(bool full)
		{
			Teacher? teacher = _loadDB.DB.Teachers.FirstOrDefault(u => u.Login == User.Identity.Name);
			List<Attendance> attendances = new List<Attendance>();
			List<Faculty> faculties = _loadDB.DB.Faculties.ToList();
			List<Class> classes = _loadDB.DB.Classes.ToList();
			List<Subject> Subjects = _loadDB.DB.Subjects.ToList();
			List<Teacher> teachers = _loadDB.DB.Teachers.ToList();

			foreach (var classa in classes)
			{
				classa.Subject = Subjects.FirstOrDefault(u => u.SubjectID == classa.SubjectID);
			}

			foreach (var attendance in (full) ? _loadDB.DB.Attendances.ToList() : _loadDB.DB.Attendances.Where(u => u.StudentID == teacher.TeacherID).ToList())
			{
				attendance.Teacher = (full) ? teachers.FirstOrDefault(u => u.TeacherID == attendance.TeacherID) : teacher;
				attendance.Class = classes.FirstOrDefault(u => u.ClassID == attendance.ClassID);
				/*attendance.Student = faculties.FirstOrDefault(u => u.StudentID == attendance.StudentID);*/
				attendances.Add(attendance);
			}

			return attendances;
		}

		[NonAction]
		private List<Attendance> GetSearch(AttendanceSearchModel model, bool full)
		{
			HttpContext.Session.SetString("Attendance_id", (model.AttendanceID != null) ? model.AttendanceID : "");
			HttpContext.Session.SetString("Attendance_classa", (model.Class != null) ? model.Class : "");
			HttpContext.Session.SetString("Attendance_isPresent", (model.IsPresent != null) ? model.IsPresent : "");
			HttpContext.Session.SetString("Attendance_type", (model.Student != null) ? model.Student : "");
			HttpContext.Session.SetString("Attendance_teacher", (model.Teacher != null) ? model.Teacher : "");

			List<Attendance> attendanceData = GetAttendances(full);
			List<Attendance> attendances = new List<Attendance>();
			List<Attendance>? filter = null;

			if (model.AttendanceID == null && model.Class == null && model.IsPresent == null && model.Student == null)
				attendances = attendanceData;
			else
			{
				if (model.AttendanceID != null)
				{
					filter = attendanceData.Where(u => Regex.IsMatch(u.AttendanceID.ToString(), model.AttendanceID)).ToList();

					if (filter != null)
						attendances.AddRange(filter);
				}

				if (model.Class != null)
				{
					if (attendances.Count != 0)
					{
						filter = attendances.Where(u => Regex.IsMatch(u.Class.Theme, model.Class)).ToList();
						attendances.Clear();
					}
					else
						filter = attendanceData.Where(u => Regex.IsMatch(u.Class.Theme, model.Class)).ToList();

					if (filter != null)
						attendances.AddRange(filter);
				}

				if (model.IsPresent != null)
				{
					if (attendances.Count != 0)
					{
						filter = attendances.Where(u => Regex.IsMatch(u.IsPresent.ToString(), model.IsPresent)).ToList();
						attendances.Clear();
					}
					else
						filter = attendanceData.Where(u => Regex.IsMatch(u.IsPresent.ToString(), model.IsPresent)).ToList();

					if (filter != null)
						attendances.AddRange(filter);
				}

				if (model.Student != null)
				{
					if (attendances.Count != 0)
					{
						filter = attendances.Where(u => Regex.IsMatch(u.Student.FirstName, model.Student)).ToList();
						attendances.Clear();
					}
					else
						filter = attendanceData.Where(u => Regex.IsMatch(u.Student.FirstName, model.Student)).ToList();

					if (filter != null)
						attendances.AddRange(filter);
				}
			}
			return attendances;
		}

	}
}
