using EnflorarteTopiProyecto.Service;
using Microsoft.AspNetCore.Mvc;

namespace EnflorarteTopiProyecto.Controllers
{
    public class ControladorSesion : Controller
    {
        private readonly ApplicationDbContext _context;

        public ControladorSesion(ApplicationDbContext context)
        {
            _context = context;
        }

        // Página de inicio de sesión (temporal): muestra usuarios de la BD
        public IActionResult Index()
        {
            var usuarios = _context.Usuarios
                .OrderByDescending(u => u.Id)
                .ToList();

            return View(usuarios);
        }
    }
}