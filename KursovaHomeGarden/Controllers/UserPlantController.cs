using Microsoft.AspNetCore.Mvc;

namespace KursovaHomeGarden.Controllers
{
    public class UserPlantController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
