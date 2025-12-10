using EnflorarteTopiProyecto.Models;
using EnflorarteTopiProyecto.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace EnflorarteTopiProyecto.Controllers
{
    public class ControladorComandas : Controller
    {
        const int MAX_IMAGEN_BYTES = 20 * 1024 * 1024; // Tamaño máximo de imagen permitida.

        private readonly ApplicationDbContext context;

        public ControladorComandas(ApplicationDbContext context)
        {
            this.context = context;
        }

        private (bool ok, string? rutaPublica, string? error) ProcesarImagen(IFormFile archivo, int maxBytes)
        {
            if (archivo == null || archivo.Length <= 0)
            {
                return (false, null, "No se proporcionó archivo.");
            }

            var permittedContentTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
            var permittedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var ext = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            var contentTypeOk = archivo.ContentType != null && permittedContentTypes.Contains(archivo.ContentType);
            var extensionOk = permittedExtensions.Contains(ext);

            if (!contentTypeOk && !extensionOk)
            {
                return (false, null, "Solo se permiten imágenes (jpg, jpeg, png, webp, gif).");
            }

            if (archivo.Length > maxBytes)
            {
                return (false, null, "La imagen supera el tamaño permitido (20 MB).");
            }

            var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsRoot))
            {
                Directory.CreateDirectory(uploadsRoot);
            }

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploadsRoot, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                archivo.CopyTo(stream);
            }

            var rutaPublica = $"/uploads/{fileName}";
            return (true, rutaPublica, null);
        }

        private bool ComprobarImagenSubida(ComandaDto comandaDto)
        {
            return (comandaDto.FotoArregloArchivo != null && comandaDto.FotoArregloArchivo.Length > 0);
        }

        public IActionResult Index()
        {
            var comandas = context.Comandas.OrderByDescending(comanda => comanda.Id).ToList();
            return View(comandas);
        }

        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(ComandaDto comandaDto)
        {
            if (!ModelState.IsValid)
            {
                return View(comandaDto);
            }

            // Validar que el usuario creador exista
            var usuarioExiste = context.Usuarios.Any(u => u.Id == comandaDto.UsuarioId);
            if (!usuarioExiste)
            {
                ModelState.AddModelError(nameof(comandaDto.UsuarioId), "El usuario especificado no existe.");
                TempData["Toast.Message"] = "Usuario especificado no existe.";
                TempData["Toast.Type"] = "warning";
                return View(comandaDto);
            }

            // Validar que el repartidor exista si se proporcionó
            if (comandaDto.RepartidorId.HasValue)
            {
                var repartidorExiste = context.Usuarios.Any(u => u.Id == comandaDto.RepartidorId.Value);
                if (!repartidorExiste)
                {
                    ModelState.AddModelError(nameof(comandaDto.RepartidorId), "El repartidor especificado no existe.");
                    TempData["Toast.Message"] = "Repartidor especificado no existe.";
                    TempData["Toast.Type"] = "warning";
                    return View(comandaDto);
                }
            }

            if (ComprobarImagenSubida(comandaDto))
            {
                var res = ProcesarImagen(comandaDto.FotoArregloArchivo, MAX_IMAGEN_BYTES);
                if (!res.ok)
                {
                    ModelState.AddModelError(nameof(comandaDto.FotoArregloArchivo), res.error!);
                    TempData["Toast.Message"] = res.error!;
                    TempData["Toast.Type"] = "warning";
                    return View(comandaDto);
                }

                comandaDto.FotoArregloRuta = res.rutaPublica;
            }

            var comandaNueva = new Comanda
            {
                UsuarioId = comandaDto.UsuarioId,
                RepartidorId = comandaDto.RepartidorId,
                ClienteNombre = comandaDto.ClienteNombre,
                ClienteTelefono = comandaDto.ClienteTelefono,
                TipoEntrega = comandaDto.TipoEntrega,
                DireccionEntrega = comandaDto.DireccionEntrega,
                FechaEntrega = comandaDto.FechaEntrega,
                HoraEntrega = comandaDto.HoraEntrega,
                NombreArreglo = comandaDto.NombreArreglo,
                PrecioArreglo = comandaDto.PrecioArreglo,
                PagoEnvio = comandaDto.PagoEnvio,
                FotoArregloRuta = comandaDto.FotoArregloRuta,
                AnticipoTipo = comandaDto.AnticipoTipo,
                AnticipoPagoTotal = comandaDto.AnticipoPagoTotal
            };

            context.Comandas.Add(comandaNueva);
            context.SaveChanges();

            TempData["Toast.Message"] = "Comanda creada correctamente.";
            TempData["Toast.Type"] = "success";

            return RedirectToAction("Index");

        }

        [Authorize(Roles = "supervisor")]
        public IActionResult Editar(int id)
        {
            var comandaAEditar = context.Comandas.Find(id);
            if (comandaAEditar == null)
            {
                TempData["Toast.Message"] = "Comanda no encontrada.";
                TempData["Toast.Type"] = "error";
                return RedirectToAction("Index");
            }

            var comandaDto = new ComandaDto
            {
                Id = id,
                UsuarioId = comandaAEditar.UsuarioId,
                RepartidorId = comandaAEditar.RepartidorId,
                ClienteNombre = comandaAEditar.ClienteNombre,
                ClienteTelefono = comandaAEditar.ClienteTelefono,
                TipoEntrega = comandaAEditar.TipoEntrega,
                DireccionEntrega = comandaAEditar.DireccionEntrega,
                FechaEntrega = comandaAEditar.FechaEntrega,
                HoraEntrega = comandaAEditar.HoraEntrega,
                NombreArreglo = comandaAEditar.NombreArreglo,
                PrecioArreglo = comandaAEditar.PrecioArreglo,
                PagoEnvio = comandaAEditar.PagoEnvio,
                FotoArregloRuta = comandaAEditar.FotoArregloRuta,
                AnticipoTipo = comandaAEditar.AnticipoTipo,
                AnticipoPagoTotal = comandaAEditar.AnticipoPagoTotal
            };

            return View(comandaDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "supervisor")]
        public IActionResult Editar(int id, ComandaDto comandaDto)
        {
            var comandaExistente = context.Comandas.Find(id);
            if (comandaExistente == null)
            {
                TempData["Toast.Message"] = "Comanda no encontrada.";
                TempData["Toast.Type"] = "error";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                return View(comandaDto);
            }

            // Validar que el usuario creador exista
            var usuarioExiste = context.Usuarios.Any(u => u.Id == comandaDto.UsuarioId);
            if (!usuarioExiste)
            {
                ModelState.AddModelError(nameof(comandaDto.UsuarioId), "El usuario especificado no existe.");
                TempData["Toast.Message"] = "Usuario especificado no existe.";
                TempData["Toast.Type"] = "warning";
                return View(comandaDto);
            }

            // Validar que el repartidor exista si se proporcionó
            if (comandaDto.RepartidorId.HasValue)
            {
                var repartidorExiste = context.Usuarios.Any(u => u.Id == comandaDto.RepartidorId.Value);
                if (!repartidorExiste)
                {
                    ModelState.AddModelError(nameof(comandaDto.RepartidorId), "El repartidor especificado no existe.");
                    TempData["Toast.Message"] = "Repartidor especificado no existe.";
                    TempData["Toast.Type"] = "warning";
                    return View(comandaDto);
                }
            }

            if (ComprobarImagenSubida(comandaDto))
            {
                var res = ProcesarImagen(comandaDto.FotoArregloArchivo, MAX_IMAGEN_BYTES);
                if (!res.ok)
                {
                    ModelState.AddModelError(nameof(comandaDto.FotoArregloArchivo), res.error!);
                    return View(comandaDto);
                }

                comandaDto.FotoArregloRuta = res.rutaPublica;
            }
            else
            {
                if (comandaDto.EliminarFoto == true)
                {
                    comandaDto.FotoArregloRuta = null;
                }
                else
                {
                    comandaDto.FotoArregloRuta = comandaExistente.FotoArregloRuta;
                }
            }

            // Actualizar los campos de la comanda existente.
            comandaExistente.UsuarioId = comandaDto.UsuarioId;
            comandaExistente.RepartidorId = comandaDto.RepartidorId;
            comandaExistente.ClienteNombre = comandaDto.ClienteNombre;
            comandaExistente.ClienteTelefono = comandaDto.ClienteTelefono;
            comandaExistente.TipoEntrega = comandaDto.TipoEntrega;
            comandaExistente.DireccionEntrega = comandaDto.DireccionEntrega;
            comandaExistente.FechaEntrega = comandaDto.FechaEntrega;
            comandaExistente.HoraEntrega = comandaDto.HoraEntrega;
            comandaExistente.NombreArreglo = comandaDto.NombreArreglo;
            comandaExistente.PrecioArreglo = comandaDto.PrecioArreglo;
            comandaExistente.PagoEnvio = comandaDto.PagoEnvio;
            comandaExistente.FotoArregloRuta = comandaDto.FotoArregloRuta;
            comandaExistente.AnticipoTipo = comandaDto.AnticipoTipo;
            comandaExistente.AnticipoPagoTotal = comandaDto.AnticipoPagoTotal;

            context.SaveChanges();

            TempData["Toast.Message"] = "Comanda actualizada correctamente.";
            TempData["Toast.Type"] = "success";

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "supervisor")]
        public IActionResult Eliminar(int id)
        {
            var comandaAEliminar = context.Comandas.Find(id);
            if (comandaAEliminar != null)
            {
                context.Comandas.Remove(comandaAEliminar);
                context.SaveChanges();

                TempData["Toast.Message"] = "Comanda eliminada correctamente.";
                TempData["Toast.Type"] = "success";
            }
            else
            {
                TempData["Toast.Message"] = "Comanda no encontrada.";
                TempData["Toast.Type"] = "error";
            }

            return RedirectToAction("Index");
        }
    }
}
