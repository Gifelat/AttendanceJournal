using ASP_Server.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using ASP_Server.Models;
using ASP_Server.CachedTables;
using AttendanceJournalLibrary;

namespace ASP_Server.Controllers
{

    public class AccountController : Controller
    {
        private readonly ILoadingDB _loadDB;
        private TeachersCachService _cache;
        public AccountController(ILoadingDB loadDB, TeachersCachService cache)
        {
            _loadDB = loadDB;
            _cache = cache;
        }

        [Route("Account")]
        public IActionResult Main()
        {
            Teacher? teacher = null;
            if (User.Identity.IsAuthenticated)
            {
                teacher = GetTeachers().FirstOrDefault(u => u.Login == User.Identity.Name);
                teacher.Role = _loadDB.DB.Roles.FirstOrDefault(u => u.RoleID == teacher.RoleID);
                if (teacher == null)
                    return RedirectToAction("Exit", "Account");
            }
            return View(teacher);
        }

        [HttpGet]
        public IActionResult SignIn() => View();

        [HttpPost]
        public async Task<IActionResult> SignIn(string login, string password)
        {
            if (!User.Identity.IsAuthenticated)
            {
                if (IsAccount(login))
                {
                    if (IsPassword(login, password))
                    {
                        AttendanceJournalLibrary.AttendanceJournalContext context = _loadDB.DB;
                        Teacher? teacher = context.Teachers.FirstOrDefault(u => u.Login == login && u.Password == password);
                        teacher.Role = context.Roles.FirstOrDefault(u => u.RoleID == teacher.RoleID);
                        await Authenticate(teacher);

                        return RedirectToAction("Main", "Account");
                    }
                    else
                        ViewData["Message"] = "Неверный пароль";
                }
                else
                    ViewData["Message"] = "Аккаунт не найден";

                return View();
            }
            else
                return RedirectToAction("Main", "Account");

        }

        [HttpGet]
        public IActionResult SignUp() => View();

        [HttpPost]
        public async Task<IActionResult> SignUp(Teacher teacher)
        {
            if (!User.Identity.IsAuthenticated)
            {
                if (ModelState.IsValid && IsLogin(teacher) && IsEmail(teacher))
                {
                    teacher.RoleID = 2;
                    await _cache.AddTeacher(teacher);
                    await Authenticate(teacher);
                    return RedirectToAction("Main", "Account");
                }
                return View();
            }
            else
                return RedirectToAction("Main", "Account");
        }

        [Authorize]
        public IActionResult EditPassword() => View();

        [Authorize]
        [HttpPut]
        public IActionResult EditPasswordAccount([FromBody] PasswordConfirmationModel passwordConfirm)
        {
            if (!IsPassword(passwordConfirm))
                return Json(new { passwordError = "Неверный пароль" });

            if (ModelState.IsValid)
            {
                Teacher? teacher = _loadDB.DB.Teachers.FirstOrDefault(u => u.Login == User.Identity.Name);
                teacher.Password = passwordConfirm.NewPassword;
                _cache.UpdateTeacher(teacher);
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [Authorize]
        public IActionResult Edit() => View(GetTeachers().FirstOrDefault(u => u.Login == User.Identity.Name));

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> EditAccount([FromBody] Teacher teacher)
        {
            if (!IsEmail(teacher))
                return Json(new { emailError = "Email уже используется" });

            if (!IsLogin(teacher))
                return Json(new { loginError = "Login уже используется" });

            if (ModelState.IsValid)
            {
                await _cache.UpdateTeacher(teacher);
                await Authenticate(teacher);
                return Json(new { success = true });
            }
            else
                return Json(new { success = false });
        }

        [Authorize]
        public async Task<IActionResult> Exit()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Main", "Account");
        }

        [NonAction]
        public async Task Authenticate(Teacher teacher)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, teacher.Login),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, _loadDB.DB.Roles.Where(u => u.RoleID == teacher.RoleID).First().Name)
            };

            ClaimsIdentity identity = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        }

        [NonAction]
        private List<Teacher> GetTeachers()
        {
            List<Teacher> teachers = new List<Teacher>();
            List<Role> roles = _loadDB.DB.Roles.ToList();

            foreach (var teacher in _cache.GetTeachers())
            {
                teacher.Role = roles.FirstOrDefault(u => u.RoleID == teacher.RoleID);
                teachers.Add(teacher);
            }

            return teachers;
        }

        [NonAction]
        private bool IsAccount(string login)
        {
            Teacher? teacher = _cache.GetTeachers().FirstOrDefault(u => u.Login == login);
            if (teacher != null)
                return true;

            return false;
        }

        [NonAction]
        private bool IsPassword(PasswordConfirmationModel passwordConfirmation)
        {
            Teacher? teacher = _cache.GetTeachers().FirstOrDefault(u => u.Login == User.Identity.Name);
            if (passwordConfirmation.Password.Equals(teacher.Password))
                return true;

            return false;
        }

        [NonAction]
        private bool IsPassword(string login, string password)
        {
            Teacher? teacher = _cache.GetTeachers().FirstOrDefault(u => u.Login == login);
            if (teacher.Password.Equals(password))
                return true;

            return false;
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
