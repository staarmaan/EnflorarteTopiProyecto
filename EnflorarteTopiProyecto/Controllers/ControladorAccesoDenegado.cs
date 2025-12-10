using Microsoft.AspNetCore.Mvc;

namespace EnflorarteTopiProyecto.Controllers
{
    public class ControladorAccesoDenegado : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View("~/Views/AccesoDenegado/Index.cshtml");
        }
    }
}
