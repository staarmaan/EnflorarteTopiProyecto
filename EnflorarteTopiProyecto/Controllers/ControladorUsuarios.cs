using EnflorarteTopiProyecto.Models;
using EnflorarteTopiProyecto.Service;
using EnflorarteTopiProyecto.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace EnflorarteTopiProyecto.Controllers
{
    [Authorize(Roles = "supervisor")] // Solo los supervisores pueden manejar usuarios.
    public class ControladorUsuarios : Controller
    {
        private readonly ApplicationDbContext context;

        public ControladorUsuarios(ApplicationDbContext context)
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
        [ValidateAntiForgeryToken]
        public IActionResult Crear(UsuarioDto usuarioDto)
        {
            if (string.IsNullOrWhiteSpace(usuarioDto.Contrasena))
            {
                ModelState.AddModelError(nameof(usuarioDto.Contrasena), "La contraseña es obligatoria.");
            }

            if (string.IsNullOrWhiteSpace(usuarioDto.ConfirmarContrasena))
            {
                ModelState.AddModelError(nameof(usuarioDto.ConfirmarContrasena), "Es obligatorio confirmar la contraseña.");
            }

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
                Id = usuarioAEditar.Id,
                Nombre = usuarioAEditar.Nombre,
                Rol = usuarioAEditar.Rol,
                Contrasena = "", // No se llena la contraseña por seguridad.
                ConfirmarContrasena = "",
                CambiarContrasena = false,
                ContrasenaSupervisor = "",
                Activo = usuarioAEditar.Activo
            };

            return View(usuarioDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int id, UsuarioDto usuarioDto)
        {
            var usuarioAEditar = context.Usuarios.Find(id);
            if (usuarioAEditar == null)
            {
                TempData["Toast.Message"] = "Usuario no encontrado.";
                TempData["Toast.Type"] = "warning";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(usuarioDto.ContrasenaSupervisor))
            {
                ModelState.AddModelError(nameof(usuarioDto.ContrasenaSupervisor), "Debes ingresar tu contraseña de supervisor para confirmar.");
            }
            else if (!ValidarContrasenaSupervisor(usuarioDto.ContrasenaSupervisor))
            {
                ModelState.AddModelError(nameof(usuarioDto.ContrasenaSupervisor), "La contraseña del supervisor es incorrecta.");
            }

            if (usuarioDto.CambiarContrasena)
            {
                if (string.IsNullOrWhiteSpace(usuarioDto.Contrasena))
                {
                    ModelState.AddModelError(nameof(usuarioDto.Contrasena), "La nueva contraseña es obligatoria.");
                }

                if (string.IsNullOrWhiteSpace(usuarioDto.ConfirmarContrasena))
                {
                    ModelState.AddModelError(nameof(usuarioDto.ConfirmarContrasena), "Debes confirmar la nueva contraseña.");
                }
            }
            else
            {
                ModelState.Remove(nameof(usuarioDto.Contrasena));
                ModelState.Remove(nameof(usuarioDto.ConfirmarContrasena));
            }

            if (!ModelState.IsValid)
            {
                return View(usuarioDto);
            }

            // Editar usuario
            usuarioAEditar.Nombre = usuarioDto.Nombre;
            usuarioAEditar.Rol = usuarioDto.Rol;
            usuarioAEditar.Activo = usuarioDto.Activo;

            if (usuarioDto.CambiarContrasena)
            {
                usuarioAEditar.Contrasena = HasheadorContrasenas.HashearContrasena(usuarioDto.Contrasena);
            }

            context.SaveChanges();

            TempData["Toast.Message"] = "Usuario actualizado correctamente.";
            TempData["Toast.Type"] = "success";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id, string contrasenaSupervisor)
        {
            if (string.IsNullOrWhiteSpace(contrasenaSupervisor) || !ValidarContrasenaSupervisor(contrasenaSupervisor))
            {
                TempData["Toast.Message"] = "Contraseña de supervisor incorrecta.";
                TempData["Toast.Type"] = "warning";
                return RedirectToAction("Index");
            }

            var usuarioAEliminar = context.Usuarios.Find(id);
            if (usuarioAEliminar == null)
            {
                TempData["Toast.Message"] = "Usuario no encontrado.";
                TempData["Toast.Type"] = "warning";
                return RedirectToAction("Index");
            }

            var respaldo = new UsuarioEliminadoDto
            {
                Nombre = usuarioAEliminar.Nombre,
                Rol = usuarioAEliminar.Rol,
                Contrasena = usuarioAEliminar.Contrasena,
                Activo = usuarioAEliminar.Activo
            };

            context.Usuarios.Remove(usuarioAEliminar);
            context.SaveChanges();

            TempData["Usuarios.UltimoEliminado"] = JsonSerializer.Serialize(respaldo);
            TempData["Toast.Message"] = $"Usuario '{respaldo.Nombre}' eliminado.";
            TempData["Toast.Type"] = "warning";
            TempData["Toast.UndoAction"] = Url.Action("DeshacerEliminacion", "ControladorUsuarios");
            TempData["Toast.UndoText"] = "Deshacer";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeshacerEliminacion()
        {
            var payload = TempData["Usuarios.UltimoEliminado"] as string;
            if (string.IsNullOrWhiteSpace(payload))
            {
                TempData["Toast.Message"] = "Ya no hay una eliminación reciente para deshacer.";
                TempData["Toast.Type"] = "info";
                return RedirectToAction("Index");
            }

            UsuarioEliminadoDto? respaldo;
            try
            {
                respaldo = JsonSerializer.Deserialize<UsuarioEliminadoDto>(payload);
            }
            catch
            {
                respaldo = null;
            }

            if (respaldo == null)
            {
                TempData["Toast.Message"] = "No se pudo deshacer la eliminación.";
                TempData["Toast.Type"] = "warning";
                return RedirectToAction("Index");
            }

            var usuarioRestaurado = new Usuario
            {
                Nombre = respaldo.Nombre,
                Rol = respaldo.Rol,
                Contrasena = respaldo.Contrasena,
                Activo = respaldo.Activo
            };

            context.Usuarios.Add(usuarioRestaurado);
            context.SaveChanges();

            TempData["Toast.Message"] = $"Se restauró el usuario '{usuarioRestaurado.Nombre}'.";
            TempData["Toast.Type"] = "success";

            return RedirectToAction("Index");
        }

        private bool ValidarContrasenaSupervisor(string contrasenaIngresada)
        {
            var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(usuarioIdClaim, out var supervisorId))
            {
                return false;
            }

            var supervisor = context.Usuarios.Find(supervisorId);
            if (supervisor == null)
            {
                return false;
            }

            bool valida;
            try
            {
                valida = HasheadorContrasenas.VerificarContrasena(supervisor.Contrasena, contrasenaIngresada);
            }
            catch
            {
                valida = false;
            }

            if (!valida && supervisor.Contrasena == contrasenaIngresada)
            {
                supervisor.Contrasena = HasheadorContrasenas.HashearContrasena(contrasenaIngresada);
                context.SaveChanges();
                valida = true;
            }

            return valida;
        }

        private class UsuarioEliminadoDto
        {
            public string Nombre { get; set; } = string.Empty;
            public RolUsuario Rol { get; set; }
            public string Contrasena { get; set; } = string.Empty;
            public bool Activo { get; set; }
        }
    }
}
