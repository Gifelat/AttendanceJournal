using Microsoft.AspNetCore.Mvc;
using ASP_Server.Services;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using ASP_Server.CachedTables;
using ASP_Server.Models;
using System.Data;
using AttendanceJournalLibrary;

namespace ASP_Server.Controllers
{
	[Authorize(Roles = "Admin")]
	public class TeachersController : Controller
	{
		private readonly ILoadingDB _loadDB;
		private TeachersCachService _cache;
		public TeachersController(ILoadingDB loadingDB, TeachersCachService cache)
		{
			_loadDB = loadingDB;
			_cache = cache;
		}

		[Route("Teachers")]
		public IActionResult Main() => View();

		[ResponseCache(Location = ResponseCacheLocation.Any, Duration = 2 * 6 + 240)]
		public IActionResult All() => View("Output", GetTeachers());

		[HttpGet]
		[ResponseCache(Location = ResponseCacheLocation.Any, Duration = 2 * 6 + 240)]
		public IActionResult Result(string id, string firstname, string lastname, string middlename, string position, string email, string login, string password, string role)
		{
			TeacherSearchModel model = new TeacherSearchModel()
			{
				TeacherID = id,
				FirstName = firstname,
				LastName = lastname,
				MiddleName = middlename,
				Position = position,
				Login = login,
				Email = email,
				Password = password,
				Role = role
			};

			return View("Output", GetSearch(model));
		}

		public IActionResult Search()
		{
			TeacherSearchModel model = new TeacherSearchModel()
			{
				TeacherID = HttpContext.Session.GetString("Teacher_id"),
				FirstName = HttpContext.Session.GetString("Teacher_firstname"),
				LastName = HttpContext.Session.GetString("Teacher_lastname"),
				MiddleName = HttpContext.Session.GetString("Teacher_middlename"),
				Position = HttpContext.Session.GetString("Teacher_position"),
				Login = HttpContext.Session.GetString("Teacher_login"),
				Email = HttpContext.Session.GetString("Teacher_email"),
				Password = HttpContext.Session.GetString("Teacher_password"),
				Role = HttpContext.Session.GetString("Teacher_role")
			};
			return View(model);
		}

		public IActionResult Add()
		{
			ViewBag.Roles = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_loadDB.DB.Roles.ToList(), "RoleID", "Name");
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddTeacher(Teacher teacher)
		{
			ViewBag.Roles = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_loadDB.DB.Roles.ToList(), "RoleID", "Name");

			if (ModelState.IsValid)
			{
				await _cache.AddTeacher(teacher);
				return View("Main");
			}
			else
				return View("Add");
		}

		[HttpGet]
		public IActionResult Edit(int id)
		{
			Teacher? teacher = _cache.GetTeacher(id);
			if (teacher != null)
			{
				ViewBag.Roles = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_loadDB.DB.Roles.ToList(), "RoleID", "Name");
				return View(teacher);
			}
			else
				return RedirectToAction("ErrorPage", "Home");
		}

		[HttpPut]
		public IActionResult EditTeacher([FromBody] Teacher teacher)
		{
			if (!IsEmail(teacher))
				return Json(new { emailError = "Email уже используется" });

			if (!IsLogin(teacher))
				return Json(new { loginError = "Login уже используется" });

			if (ModelState.IsValid)
			{
				Teacher? thisTeacher = _loadDB.DB.Teachers.FirstOrDefault(u => u.Login == User.Identity.Name);
				if (thisTeacher != null && thisTeacher.TeacherID == teacher.TeacherID)
					HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();

				_cache.UpdateTeacher(teacher);
				return Json(new { success = true });
			}
			else
				return Json(new { success = false });
		}

		[HttpGet]
		public IActionResult Delete(int id)
		{
			Teacher? teacher = _cache.GetTeacher(id);
			ViewData["Message"] = (teacher == null) ? $"Запись №{id} удалена!" : "Запись не удалена или не найдена";
			return View();
		}

		[HttpDelete]
		public async Task<IActionResult> DeleteTeacher(int id)
		{
			try
			{
				Teacher? thisTeacher = _loadDB.DB.Teachers.FirstOrDefault(u => u.Login == User.Identity.Name);
				Teacher? teacher = _cache.GetTeacher(id);

				if (thisTeacher != null && teacher != null && thisTeacher.TeacherID == teacher.TeacherID)
					HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();

				await _cache.DeleteTeacher(teacher);
				return Json(new { success = true });
			}
			catch
			{
				return Json(new { success = false });
			}
		}

		[NonAction]
		private List<Teacher> GetTeachers()
		{
			List<Teacher> teachers = new List<Teacher>();
			List<Role> roles = _loadDB.DB.Roles.ToList();

			foreach (var teacher in _cache.GetTeachers())
			{
				teacher.Role = roles.Where(u => u.RoleID == teacher.RoleID).First();
				teachers.Add(teacher);
			}

			return teachers;
		}

		[NonAction]
		private List<Teacher> GetSearch(TeacherSearchModel model)
		{
			HttpContext.Session.SetString("Teacher_id", (model.TeacherID != null)? model.TeacherID : "");
			HttpContext.Session.SetString("Teacher_firstname", (model.FirstName != null) ? model.FirstName : "");
			HttpContext.Session.SetString("Teacher_lastname", (model.LastName != null) ? model.LastName : "");
			HttpContext.Session.SetString("Teacher_middlename", (model.MiddleName != null) ? model.MiddleName : "");
			HttpContext.Session.SetString("Teacher_position", (model.Position != null) ? model.Position : "");
			HttpContext.Session.SetString("Teacher_email", (model.Email != null) ? model.Email : "");
			HttpContext.Session.SetString("Teacher_login", (model.Login != null) ? model.Login : "");
			HttpContext.Session.SetString("Teacher_password", (model.Password != null) ? model.Password : "");
			HttpContext.Session.SetString("Teacher_role", (model.Role != null) ? model.Role : "");

			List<Teacher> teacherData = GetTeachers();
			List<Teacher> teachers = new List<Teacher>();
			List<Teacher>? filter = null;

			if (model.TeacherID == null && model.FirstName == null && model.LastName == null && model.Email == null && model.Login == null && model.Password == null && model.Role == null)
				teachers = teacherData;
			else
			{
				if (model.TeacherID != null)
				{
					filter = teacherData.Where(u => Regex.IsMatch(u.TeacherID.ToString(), model.TeacherID)).ToList();

					if (filter != null)
						teachers.AddRange(filter);
				}

				if (model.FirstName != null)
				{
					if (teachers.Count != 0)
					{
						filter = teachers.Where(u => Regex.IsMatch(u.FirstName, model.FirstName)).ToList();
						teachers.Clear();
					}
					else
						filter = teacherData.Where(u => Regex.IsMatch(u.FirstName, model.FirstName)).ToList();

					if (filter != null)
						teachers.AddRange(filter);
				}

				if (model.LastName != null)
				{
					if (teachers.Count != 0)
					{
						filter = teachers.Where(u => Regex.IsMatch(u.LastName, model.LastName)).ToList();
						teachers.Clear();
					}
					else
						filter = teacherData.Where(u => Regex.IsMatch(u.LastName, model.LastName)).ToList();

					if (filter != null)
						teachers.AddRange(filter);
				}

				if (model.MiddleName != null)
				{
					if (teachers.Count != 0)
					{
						filter = teachers.Where(u => Regex.IsMatch(u.MiddleName, model.MiddleName)).ToList();
						teachers.Clear();
					}
					else
						filter = teacherData.Where(u => Regex.IsMatch(u.MiddleName, model.MiddleName)).ToList();

					if (filter != null)
						teachers.AddRange(filter);
				}

				if (model.Position != null)
				{
					if (teachers.Count != 0)
					{
						filter = teachers.Where(u => Regex.IsMatch(u.Position, model.Position)).ToList();
						teachers.Clear();
					}
					else
						filter = teacherData.Where(u => Regex.IsMatch(u.Position, model.Position)).ToList();

					if (filter != null)
						teachers.AddRange(filter);
				}

				if (model.Email != null)
				{
					if (teachers.Count != 0)
					{
						filter = teachers.Where(u => Regex.IsMatch(u.Email, model.Email)).ToList();
						teachers.Clear();
					}
					else
						filter = teacherData.Where(u => Regex.IsMatch(u.Email, model.Email)).ToList();

					if (filter != null)
						teachers.AddRange(filter);
				}

				if (model.Login != null)
				{
					if (teachers.Count != 0)
					{
						filter = teachers.Where(u => Regex.IsMatch(u.Login, model.Login)).ToList();
						teachers.Clear();
					}
					else
						filter = teacherData.Where(u => Regex.IsMatch(u.Login, model.Login)).ToList();

					if (filter != null)
						teachers.AddRange(filter);
				}

				if (model.Password != null)
				{
					if (teachers.Count != 0)
					{
						filter = teachers.Where(u => Regex.IsMatch(u.Password, model.Password)).ToList();
						teachers.Clear();
					}
					else
						filter = teacherData.Where(u => Regex.IsMatch(u.Password, model.Password)).ToList();

					if (filter != null)
						teachers.AddRange(filter);
				}

				if (model.Role != null)
				{
					if (teachers.Count != 0)
					{
						filter = teachers.Where(u => Regex.IsMatch(u.Role.Name, model.Role)).ToList();
						teachers.Clear();
					}
					else
						filter = teacherData.Where(u => Regex.IsMatch(u.Role.Name, model.Role)).ToList();

					if (filter != null)
						teachers.AddRange(filter);
				}
			}
			return teachers;
		}

		[NonAction]
		private bool IsLogin(Teacher teacher)
		{
			Teacher? check = _cache.GetTeachers().FirstOrDefault(u => u.Login == teacher.Login && u.TeacherID != teacher.TeacherID);

			if (check != null)
				return false;

			return true;
		}

		[NonAction]
		private bool IsEmail(Teacher teacher)
		{
			Teacher? check = _cache.GetTeachers().FirstOrDefault(u => u.Email == teacher.Email && u.TeacherID != teacher.TeacherID);

			if (check != null)
				return false;

			return true;
		}
	}
}
