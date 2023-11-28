using ASP_Server.Services;
using ASP_Server.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using AttendanceJournalLibrary;

namespace ASP_Server.Controllers
{
	public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
		private readonly ILoadingDB _loadDB;

		public HomeController(ILogger<HomeController> logger, ILoadingDB loadDB)
        {
            _logger = logger;
			_loadDB = loadDB;
		}

        public IActionResult Index()
        {
            Teacher? teacher = _loadDB.DB.Teachers.FirstOrDefault(u => u.Login == User.Identity.Name);
            if(teacher == null)
				HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();

			return View();
        }

        [Route("Privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

		[Route("NotFound")]
		public IActionResult ErrorPage()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestID = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}