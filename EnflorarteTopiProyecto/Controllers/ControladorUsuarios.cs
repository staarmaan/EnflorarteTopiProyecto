using EnflorarteTopiProyecto.Service;
using Microsoft.AspNetCore.Mvc;

namespace EnflorarteTopiProyecto.Controllers
{
    public class ControladorUsuarios : Controller
    {
        private readonly ApplicationDbContext context;

        public ControladorUsuarios (ApplicationDbContext context)
        {
            this.context = context;
        }  
        public IActionResult Index()
        {
            var usuarios = context.Usuarios.OrderByDescending(usuario => usuario.Id).ToList();
            return View(usuarios); // No olvides pasar el modelo (usuarios, en este caso).
        }
    }
}
