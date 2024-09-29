using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers
{
    public class HomeController : Controller
    {
        private readonly IApplication _application;

        public HomeController(ILogger<HomeController> logger, IApplication application)
        {
            _application = application;
        }

        public async Task<IActionResult> Index()
        {
            await _application.Run();

            return View();
        }
    }
}
