using EnflorarteTopiProyecto.Service;
using EnflorarteTopiProyecto.Utils;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Ingresar(int usuarioId, string contrasenaIngresada)
        {
            var usuario = _context.Usuarios.Find(usuarioId);
            if (usuario == null)
            {
                TempData["Toast.Message"] = "Usuario no encontrado.";
                TempData["Toast.Type"] = "warning";
                return RedirectToAction("Index");
            }

            // Validar contraseña (la contraseña en BD está almacenada hasheada)
            bool contrasenaValida;
            try
            {
                contrasenaValida = HasheadorContrasenas.VerificarContrasena(usuario.Contrasena, contrasenaIngresada);
            }
            catch
            {
                contrasenaValida = false;
            }

            //contrasenaValida = true; // PARA TESTEAR SIN IMPORTAR LA CONTRASEÑA

            if (!contrasenaValida)
            {
                TempData["Toast.Message"] = "Contraseña incorrecta.";
                TempData["Toast.Type"] = "warning";

                //return View();
                return RedirectToAction("Index");
            }

            TempData["Toast.Message"] = "Bienvenido, " + usuario.Nombre + ".";
            TempData["Toast.Type"] = "success";
            return RedirectToAction("Index", "Home");
        }
    }
}