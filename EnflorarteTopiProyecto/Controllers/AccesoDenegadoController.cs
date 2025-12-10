using Microsoft.AspNetCore.Mvc;

namespace EnflorarteTopiProyecto.Controllers
{
    public class AccesoDenegadoController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
