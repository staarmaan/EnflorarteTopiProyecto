using EnflorarteTopiProyecto.Models;
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
            // Mostrar a los usuarios en orden de mayor a menor por Id
            var usuarios = context.Usuarios.OrderByDescending(usuario => usuario.Id).ToList();
            return View(usuarios); // No olvides pasar el modelo (usuarios, en este caso) a la vista.
        }

        public IActionResult Crear() 
        {
            return View();
        }

        [HttpPost]
        public IActionResult Crear(UsuarioDto usuarioDto)
        {
            if (!ModelState.IsValid)
            {
                return View(usuarioDto);
            }

            // Crear nuevo usuario si es valido.
            var usuarioNuevo = new Usuario
            {
                Nombre = usuarioDto.Nombre,
                Rol = usuarioDto.Rol,
                Contrasena = usuarioDto.Contrasena,
                Activo = usuarioDto.Activo
            };

            context.Usuarios.Add(usuarioNuevo);
            context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
