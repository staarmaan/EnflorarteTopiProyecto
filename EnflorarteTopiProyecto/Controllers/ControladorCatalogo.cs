using EnflorarteTopiProyecto.Models;
using EnflorarteTopiProyecto.Service;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            var flores = new List<Flor>();
            var arreglos = new List<Arreglo>();

            try
            {
                flores = context.Flores.OrderByDescending(f => f.Id).ToList();

                arreglos = context.Arreglos
                    .Include(a => a.Flores)
                    .ThenInclude(af => af.Flor)
                    .OrderByDescending(a => a.Id)
                    .ToList();
            }
            catch (Exception ex) when (EsErrorDeEsquemaArreglos(ex))
            {
                TempData["Toast.Message"] = "Falta actualizar la base de datos para habilitar arreglos (tabla 'arreglo').";
                TempData["Toast.Type"] = "warning";

                try
                {
                    flores = context.Flores.OrderByDescending(f => f.Id).ToList();
                }
                catch
                {
                    flores = new List<Flor>();
                }
            }

            var modelo = new CatalogoIndexViewModel
            {
                Flores = flores,
                Arreglos = arreglos
            };

            return View(modelo);
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

        public IActionResult CrearArreglo()
        {
            var dto = new ArregloDto
            {
                Flores = context.Flores
                    .OrderBy(f => f.Nombre)
                    .Select(f => new ArregloFlorSeleccionDto
                    {
                        FlorId = f.Id,
                        FlorNombre = f.Nombre,
                        Cantidad = 1
                    })
                    .ToList()
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CrearArreglo(ArregloDto dto)
        {
            dto.Flores ??= new List<ArregloFlorSeleccionDto>();

            if (dto.FotoArchivo == null || dto.FotoArchivo.Length <= 0)
            {
                ModelState.AddModelError(nameof(dto.FotoArchivo), "La foto es obligatoria.");
            }

            var floresCatalogo = context.Flores
                .OrderBy(f => f.Nombre)
                .Select(f => new { f.Id, f.Nombre })
                .ToList();

            var nombresPorFlorId = floresCatalogo.ToDictionary(f => f.Id, f => f.Nombre);

            if (dto.Flores.Count == 0)
            {
                dto.Flores = floresCatalogo
                    .Select(f => new ArregloFlorSeleccionDto
                    {
                        FlorId = f.Id,
                        FlorNombre = f.Nombre,
                        Cantidad = 1
                    })
                    .ToList();
            }
            else
            {
                foreach (var item in dto.Flores)
                {
                    if (nombresPorFlorId.TryGetValue(item.FlorId, out var nombreFlor))
                    {
                        item.FlorNombre = nombreFlor;
                    }
                }
            }

            var floresSeleccionadas = dto.Flores
                .Where(f => f.Seleccionada && f.Cantidad > 0)
                .ToList();

            if (floresSeleccionadas.Count == 0)
            {
                ModelState.AddModelError(nameof(dto.Flores), "Selecciona al menos una flor con cantidad mayor a 0.");
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

            var arreglo = new Arreglo
            {
                Nombre = dto.Nombre,
                FotoRuta = imagen.rutaPublica,
                Descripcion = dto.Descripcion,
                Flores = floresSeleccionadas.Select(f => new ArregloFlor
                {
                    FlorId = f.FlorId,
                    Cantidad = f.Cantidad
                }).ToList()
            };

            context.Arreglos.Add(arreglo);

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex) when (EsErrorDeEsquemaArreglos(ex))
            {
                ModelState.AddModelError(string.Empty, "No se pudo guardar el arreglo porque falta actualizar la base de datos (tabla 'arreglo').");
                return View(dto);
            }

            TempData["Toast.Message"] = "Arreglo registrado correctamente.";
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

        private static bool EsErrorDeEsquemaArreglos(Exception ex)
        {
            if (ex is SqlException sqlEx)
            {
                return sqlEx.Number == 208 &&
                       (sqlEx.Message.Contains("arreglo", StringComparison.OrdinalIgnoreCase) ||
                        sqlEx.Message.Contains("arreglo_flor", StringComparison.OrdinalIgnoreCase));
            }

            if (ex is DbUpdateException dbEx && dbEx.InnerException is SqlException innerSqlEx)
            {
                return innerSqlEx.Number == 208 &&
                       (innerSqlEx.Message.Contains("arreglo", StringComparison.OrdinalIgnoreCase) ||
                        innerSqlEx.Message.Contains("arreglo_flor", StringComparison.OrdinalIgnoreCase));
            }

            return false;
        }
    }
}
