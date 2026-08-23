using Microsoft.AspNetCore.Mvc;

namespace BikeStore.API.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}