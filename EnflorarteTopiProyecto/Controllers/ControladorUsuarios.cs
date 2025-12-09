using EnflorarteTopiProyecto.Models;
using EnflorarteTopiProyecto.Service;
using Microsoft.AspNetCore.Mvc;

using EnflorarteTopiProyecto.Utils;

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
                Contrasena = HasheadorContrasenas.HashearContrasena(usuarioDto.Contrasena),
                Activo = usuarioDto.Activo
            };

            context.Usuarios.Add(usuarioNuevo);
            context.SaveChanges();

            TempData["Toast.Message"] = "Usuario creado correctamente.";
            TempData["Toast.Type"] = "success";

            return RedirectToAction("Index");
        }

        public IActionResult Editar(int id)
        {
            var usuarioAEditar = context.Usuarios.Find(id);

            // Si el usuario no existe, redirigir a la pagina principal (index).
            if (usuarioAEditar == null)
            {
                TempData["Toast.Message"] = "Usuario no encontrado.";
                TempData["Toast.Type"] = "warning";
                return RedirectToAction("Index");
            }

            var usuarioDto = new UsuarioDto
            {
                Nombre = usuarioAEditar.Nombre,
                Rol = usuarioAEditar.Rol,
                Contrasena = "", // No se llena la contraseña por seguridad.
                Activo = usuarioAEditar.Activo
            };

            ViewBag.UsuarioId = id;

            return View(usuarioDto);
        }

        [HttpPost]
        public IActionResult Editar(int id, UsuarioDto usuarioDto)
        {
            var usuarioAEditar = context.Usuarios.Find(id);
            if (usuarioAEditar == null)
            {
                TempData["Toast.Message"] = "Usuario no encontrado.";
                TempData["Toast.Type"] = "warning";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                return View(usuarioDto);
            }

            // Editar usuario
            usuarioAEditar.Nombre = usuarioDto.Nombre;
            usuarioAEditar.Rol = usuarioDto.Rol;
            usuarioAEditar.Contrasena = HasheadorContrasenas.HashearContrasena(usuarioDto.Contrasena);
            usuarioAEditar.Activo = usuarioDto.Activo;

            context.SaveChanges();

            TempData["Toast.Message"] = "Usuario actualizado correctamente.";
            TempData["Toast.Type"] = "success";

            return RedirectToAction("Index");
        }

        public IActionResult Eliminar(int id)
        {
            var usuarioAEliminar = context.Usuarios.Find(id);
            if (usuarioAEliminar != null)
            {
                context.Usuarios.Remove(usuarioAEliminar);
                context.SaveChanges();
                TempData["Toast.Message"] = "Usuario eliminado.";
                TempData["Toast.Type"] = "info";
            }
            else 
            {
                TempData["Toast.Message"] = "Usuario no encontrado.";
                TempData["Toast.Type"] = "warning";
            }

            return RedirectToAction("Index");
        }
    }
}
