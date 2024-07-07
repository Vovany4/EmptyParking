using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers
{
    public class HomeController : Controller
    {
        public HomeController(ILogger<HomeController> logger, IApplication application)
        {
            application.Run();
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
