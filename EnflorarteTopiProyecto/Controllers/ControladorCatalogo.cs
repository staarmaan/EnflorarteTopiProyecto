using EnflorarteTopiProyecto.Models;
using EnflorarteTopiProyecto.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnflorarteTopiProyecto.Controllers
{
    [Authorize]
    public class ControladorCatalogo : Controller
    {
        private const int MAX_IMAGEN_BYTES = 10 * 1024 * 1024;
        private readonly ApplicationDbContext context;

        public ControladorCatalogo(ApplicationDbContext context)
        {
            this.context = context;
        }

        public IActionResult Index()
        {
            var flores = context.Flores.OrderByDescending(f => f.Id).ToList();
            return View(flores);
        }

        public IActionResult CrearFlor()
        {
            return View(new FlorDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CrearFlor(FlorDto dto)
        {
            if (dto.FotoArchivo == null || dto.FotoArchivo.Length <= 0)
            {
                ModelState.AddModelError(nameof(dto.FotoArchivo), "La foto es obligatoria.");
            }

            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var imagen = GuardarImagen(dto.FotoArchivo!);
            if (!imagen.ok)
            {
                ModelState.AddModelError(nameof(dto.FotoArchivo), imagen.error!);
                return View(dto);
            }

            var flor = new Flor
            {
                Nombre = dto.Nombre,
                FotoRuta = imagen.rutaPublica,
                Descripcion = dto.Descripcion
            };

            context.Flores.Add(flor);
            context.SaveChanges();

            TempData["Toast.Message"] = "Flor registrada correctamente.";
            TempData["Toast.Type"] = "success";

            return RedirectToAction("Index");
        }

        private (bool ok, string? rutaPublica, string? error) GuardarImagen(IFormFile archivo)
        {
            var permittedContentTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
            var permittedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

            var ext = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            var contentTypeOk = archivo.ContentType != null && permittedContentTypes.Contains(archivo.ContentType);
            var extensionOk = permittedExtensions.Contains(ext);

            if (!contentTypeOk && !extensionOk)
            {
                return (false, null, "Solo se permiten imágenes (jpg, jpeg, png, webp, gif).");
            }

            if (archivo.Length > MAX_IMAGEN_BYTES)
            {
                return (false, null, "La imagen supera el tamaño permitido (10 MB).");
            }

            var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "flores");
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

            return (true, $"/uploads/flores/{fileName}", null);
        }
    }
}
