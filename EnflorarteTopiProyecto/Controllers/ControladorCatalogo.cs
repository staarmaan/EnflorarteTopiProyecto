using EnflorarteTopiProyecto.Models;
using EnflorarteTopiProyecto.Service;
using EnflorarteTopiProyecto.Utils;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EnflorarteTopiProyecto.Controllers
{
    [Authorize(Roles = "supervisor")] // Solo los supervisores pueden manejan el catalogo.
    public class ControladorCatalogo : Controller
    {
        private const int MAX_IMAGEN_BYTES = 10 * 1024 * 1024;
        private static readonly string[] COLORES_BASE_INVENTARIO = { "Rojo", "Rosa Claro", "Fiusha", "Blanca", "Lila" };
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
                flores = context.Flores
                    .Include(f => f.InventarioColores)
                    .OrderByDescending(f => f.Id)
                    .ToList();

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
                    flores = context.Flores
                        .Include(f => f.InventarioColores)
                        .OrderByDescending(f => f.Id)
                        .ToList();
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

        public IActionResult InventarioGeneral()
        {
            var flores = context.Flores
                .Include(f => f.InventarioColores)
                .OrderBy(f => f.Nombre)
                .ToList();

            var coloresEnInventario = flores
                .SelectMany(f => f.InventarioColores.Select(c => c.Color))
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .ToList();

            var coloresExtras = coloresEnInventario
                .Where(c => !COLORES_BASE_INVENTARIO.Any(baseColor => string.Equals(baseColor, c, StringComparison.OrdinalIgnoreCase)))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(c => c)
                .ToList();

            var colores = COLORES_BASE_INVENTARIO
                .Concat(coloresExtras)
                .ToList();

            var modelo = new InventarioGeneralViewModel
            {
                Colores = colores,
                Filas = flores.Select(f => new InventarioGeneralFilaViewModel
                {
                    FlorId = f.Id,
                    FlorNombre = f.Nombre,
                    CantidadesPorColor = colores.ToDictionary(
                        color => color,
                        color => f.InventarioColores
                            .FirstOrDefault(i => string.Equals(i.Color, color, StringComparison.OrdinalIgnoreCase))?.Cantidad ?? 0)
                }).ToList()
            };

            return View(modelo);
        }

        public IActionResult DetalleFlor(int id)
        {
            AsegurarColoresBaseInventario(id);

            var flor = context.Flores
                .Include(f => f.InventarioColores)
                .FirstOrDefault(f => f.Id == id);
            if (flor == null)
            {
                return Content("<div class='alert alert-warning mb-0'>Flor no encontrada.</div>", "text/html");
            }

            return View(flor);
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

            var coloresIniciales = COLORES_BASE_INVENTARIO
                .Select(color => new FlorInventarioColor
                {
                    FlorId = flor.Id,
                    Color = color,
                    Cantidad = 0
                }).ToList();

            context.FloresInventarioColores.AddRange(coloresIniciales);
            context.SaveChanges();

            TempData["Toast.Message"] = "Flor registrada correctamente.";
            TempData["Toast.Type"] = "success";

            return RedirectToAction("Index");
        }

        public IActionResult CrearArreglo()
        {
            var coloresPorFlorId = ObtenerColoresPorFlorId();

            var dto = new ArregloDto
            {
                Flores = context.Flores
                    .OrderBy(f => f.Nombre)
                    .Select(f => new ArregloFlorSeleccionDto
                    {
                        FlorId = f.Id,
                        FlorNombre = f.Nombre,
                        Cantidad = 1,
                        ColorSeleccionado = "a elegir",
                        ColoresDisponibles = ObtenerOpcionesColorParaFlor(f.Id, coloresPorFlorId)
                    })
                    .ToList()
            };

            return View(dto);
        }

        public IActionResult EditarInventarioFlor(int id)
        {
            AsegurarColoresBaseInventario(id);

            var flor = context.Flores
                .Include(f => f.InventarioColores)
                .FirstOrDefault(f => f.Id == id);

            if (flor == null)
            {
                TempData["Toast.Message"] = "Flor no encontrada.";
                TempData["Toast.Type"] = "warning";
                return RedirectToAction("Index");
            }

            var dto = new EditarInventarioFlorDto
            {
                FlorId = flor.Id,
                NombreFlor = flor.Nombre,
                DescripcionFlor = flor.Descripcion,
                FotoRutaFlor = flor.FotoRuta,
                Colores = flor.InventarioColores
                    .OrderBy(c => c.Color)
                    .Select(c => new InventarioColorAjusteDto
                    {
                        InventarioId = c.Id,
                        Color = c.Color,
                        CantidadActual = c.Cantidad
                    })
                    .ToList()
            };

            return View(dto);
        }

        public IActionResult DetalleArreglo(int id)
        {
            var arreglo = context.Arreglos
                .Include(a => a.Flores)
                .ThenInclude(af => af.Flor)
                .FirstOrDefault(a => a.Id == id);

            if (arreglo == null)
            {
                return Content("<div class='alert alert-warning mb-0'>Arreglo no encontrado.</div>", "text/html");
            }

            return View(arreglo);
        }

        public IActionResult EditarArreglo(int id)
        {
            var arreglo = context.Arreglos
                .Include(a => a.Flores)
                .FirstOrDefault(a => a.Id == id);

            if (arreglo == null)
            {
                TempData["Toast.Message"] = "Arreglo no encontrado.";
                TempData["Toast.Type"] = "warning";
                return RedirectToAction("Index");
            }

            var floresCatalogo = context.Flores
                .OrderBy(f => f.Nombre)
                .Select(f => new { f.Id, f.Nombre })
                .ToList();

            var coloresPorFlorId = ObtenerColoresPorFlorId();

            var cantidades = arreglo.Flores.ToDictionary(f => f.FlorId, f => f.Cantidad);
            var coloresSeleccionados = arreglo.Flores.ToDictionary(f => f.FlorId, f => f.ColorSeleccionado);

            var dto = new ArregloDto
            {
                Nombre = arreglo.Nombre,
                FotoRutaActual = arreglo.FotoRuta,
                Descripcion = arreglo.Descripcion,
                Flores = floresCatalogo.Select(f => new ArregloFlorSeleccionDto
                {
                    FlorId = f.Id,
                    FlorNombre = f.Nombre,
                    Seleccionada = cantidades.ContainsKey(f.Id),
                    Cantidad = cantidades.TryGetValue(f.Id, out var cantidad) ? cantidad : 1,
                    ColorSeleccionado = coloresSeleccionados.TryGetValue(f.Id, out var color) ? NormalizarColorSeleccionado(color) : "a elegir",
                    ColoresDisponibles = ObtenerOpcionesColorParaFlor(f.Id, coloresPorFlorId)
                }).ToList()
            };

            ViewBag.ArregloId = id;
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CrearArreglo(ArregloDto dto)
        {
            dto.Flores ??= new List<ArregloFlorSeleccionDto>();
            var coloresPorFlorId = ObtenerColoresPorFlorId();

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
                        Cantidad = 1,
                        ColorSeleccionado = "a elegir",
                        ColoresDisponibles = ObtenerOpcionesColorParaFlor(f.Id, coloresPorFlorId)
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

                    item.ColoresDisponibles = ObtenerOpcionesColorParaFlor(item.FlorId, coloresPorFlorId);
                    item.ColorSeleccionado = NormalizarColorSeleccionado(item.ColorSeleccionado);
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
                    Cantidad = f.Cantidad,
                    ColorSeleccionado = NormalizarColorSeleccionado(f.ColorSeleccionado)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarArreglo(int id, ArregloDto dto)
        {
            var arreglo = context.Arreglos
                .Include(a => a.Flores)
                .FirstOrDefault(a => a.Id == id);

            if (arreglo == null)
            {
                TempData["Toast.Message"] = "Arreglo no encontrado.";
                TempData["Toast.Type"] = "warning";
                return RedirectToAction("Index");
            }

            dto.Flores ??= new List<ArregloFlorSeleccionDto>();
            var coloresPorFlorId = ObtenerColoresPorFlorId();

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
                        Cantidad = 1,
                        ColorSeleccionado = "a elegir",
                        ColoresDisponibles = ObtenerOpcionesColorParaFlor(f.Id, coloresPorFlorId)
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

                    item.ColoresDisponibles = ObtenerOpcionesColorParaFlor(item.FlorId, coloresPorFlorId);
                    item.ColorSeleccionado = NormalizarColorSeleccionado(item.ColorSeleccionado);
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
                dto.FotoRutaActual = arreglo.FotoRuta;
                ViewBag.ArregloId = id;
                return View(dto);
            }

            if (dto.FotoArchivo != null && dto.FotoArchivo.Length > 0)
            {
                var imagen = GuardarImagen(dto.FotoArchivo);
                if (!imagen.ok)
                {
                    ModelState.AddModelError(nameof(dto.FotoArchivo), imagen.error!);
                    dto.FotoRutaActual = arreglo.FotoRuta;
                    ViewBag.ArregloId = id;
                    return View(dto);
                }

                arreglo.FotoRuta = imagen.rutaPublica;
            }

            arreglo.Nombre = dto.Nombre;
            arreglo.Descripcion = dto.Descripcion;

            context.ArreglosFlores.RemoveRange(arreglo.Flores);
            arreglo.Flores = floresSeleccionadas.Select(f => new ArregloFlor
            {
                FlorId = f.FlorId,
                Cantidad = f.Cantidad,
                ColorSeleccionado = NormalizarColorSeleccionado(f.ColorSeleccionado)
            }).ToList();

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex) when (EsErrorDeEsquemaArreglos(ex))
            {
                ModelState.AddModelError(string.Empty, "No se pudo guardar el arreglo porque falta actualizar la base de datos (tabla 'arreglo').");
                dto.FotoRutaActual = arreglo.FotoRuta;
                ViewBag.ArregloId = id;
                return View(dto);
            }

            TempData["Toast.Message"] = "Arreglo actualizado correctamente.";
            TempData["Toast.Type"] = "success";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarInventarioFlor(EditarInventarioFlorDto dto, string? accion)
        {
            var flor = context.Flores
                .Include(f => f.InventarioColores)
                .FirstOrDefault(f => f.Id == dto.FlorId);

            if (flor == null)
            {
                TempData["Toast.Message"] = "Flor no encontrada.";
                TempData["Toast.Type"] = "warning";
                return RedirectToAction("Index");
            }

            dto.Colores ??= new List<InventarioColorAjusteDto>();
            dto.NombreFlor = flor.Nombre;
            dto.DescripcionFlor = flor.Descripcion;
            dto.FotoRutaFlor = flor.FotoRuta;

            foreach (var ajuste in dto.Colores)
            {
                if (ajuste.AgregarCantidad.HasValue && (ajuste.AgregarCantidad.Value < 1 || ajuste.AgregarCantidad.Value > 999))
                {
                    ModelState.AddModelError(string.Empty, $"La cantidad a agregar para '{ajuste.Color}' no es v�lida.");
                }

                if (ajuste.QuitarCantidad.HasValue && (ajuste.QuitarCantidad.Value < 1 || ajuste.QuitarCantidad.Value > 999))
                {
                    ModelState.AddModelError(string.Empty, $"La cantidad a quitar para '{ajuste.Color}' no es v�lida.");
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.NuevoColor) && dto.NuevoColor.Trim().Length > 50)
            {
                ModelState.AddModelError(nameof(dto.NuevoColor), "El color no debe exceder 50 caracteres.");
            }

            if (!ModelState.IsValid)
            {
                dto.Colores = flor.InventarioColores
                    .OrderBy(c => c.Color)
                    .Select(c => new InventarioColorAjusteDto
                    {
                        InventarioId = c.Id,
                        Color = c.Color,
                        CantidadActual = c.Cantidad
                    })
                    .ToList();

                return View(dto);
            }

            if (string.Equals(accion, "agregar-color", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(dto.NuevoColor))
                {
                    ModelState.AddModelError(nameof(dto.NuevoColor), "Ingresa un color para agregar.");
                }
                else
                {
                    var nuevoColor = dto.NuevoColor.Trim();
                    var existeColor = flor.InventarioColores
                        .Any(c => c.Color.ToLower() == nuevoColor.ToLower());

                    if (existeColor)
                    {
                        ModelState.AddModelError(nameof(dto.NuevoColor), "Ese color ya existe para esta flor.");
                    }
                    else
                    {
                        context.FloresInventarioColores.Add(new FlorInventarioColor
                        {
                            FlorId = flor.Id,
                            Color = nuevoColor,
                            Cantidad = 0
                        });

                        context.SaveChanges();
                        dto.NuevoColor = string.Empty;
                    }
                }

                dto.Colores = context.FloresInventarioColores
                    .Where(c => c.FlorId == flor.Id)
                    .OrderBy(c => c.Color)
                    .Select(c => new InventarioColorAjusteDto
                    {
                        InventarioId = c.Id,
                        Color = c.Color,
                        CantidadActual = c.Cantidad
                    })
                    .ToList();

                return View(dto);
            }

            if (!string.IsNullOrWhiteSpace(dto.NuevoColor))
            {
                var nuevoColor = dto.NuevoColor.Trim();
                var existeColor = flor.InventarioColores
                    .Any(c => c.Color.ToLower() == nuevoColor.ToLower());

                if (existeColor)
                {
                    ModelState.AddModelError(nameof(dto.NuevoColor), "Ese color ya existe para esta flor.");
                }
                else
                {
                    context.FloresInventarioColores.Add(new FlorInventarioColor
                    {
                        FlorId = flor.Id,
                        Color = nuevoColor,
                        Cantidad = 0
                    });
                }
            }

            foreach (var ajuste in dto.Colores)
            {
                var colorDb = flor.InventarioColores.FirstOrDefault(c => c.Id == ajuste.InventarioId);
                if (colorDb == null)
                {
                    continue;
                }

                if (ajuste.AgregarCantidad.HasValue)
                {
                    colorDb.Cantidad += ajuste.AgregarCantidad.Value;
                }

                if (ajuste.QuitarCantidad.HasValue)
                {
                    colorDb.Cantidad = Math.Max(0, colorDb.Cantidad - ajuste.QuitarCantidad.Value);
                }
            }

            if (!ModelState.IsValid)
            {
                dto.Colores = flor.InventarioColores
                    .OrderBy(c => c.Color)
                    .Select(c => new InventarioColorAjusteDto
                    {
                        InventarioId = c.Id,
                        Color = c.Color,
                        CantidadActual = c.Cantidad
                    })
                    .ToList();

                return View(dto);
            }

            context.SaveChanges();

            TempData["Toast.Message"] = "Inventario actualizado correctamente.";
            TempData["Toast.Type"] = "success";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "supervisor")]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarArreglo(int id, string contrasenaSupervisor)
        {
            if (string.IsNullOrWhiteSpace(contrasenaSupervisor) || !ValidarContrasenaSupervisor(contrasenaSupervisor))
            {
                TempData["Toast.Message"] = "Contrase�a de supervisor incorrecta.";
                TempData["Toast.Type"] = "warning";
                return RedirectToAction("Index");
            }

            var arreglo = context.Arreglos
                .Include(a => a.Flores)
                .FirstOrDefault(a => a.Id == id);

            if (arreglo == null)
            {
                TempData["Toast.Message"] = "Arreglo no encontrado.";
                TempData["Toast.Type"] = "warning";
                return RedirectToAction("Index");
            }

            context.Arreglos.Remove(arreglo);
            context.SaveChanges();

            TempData["Toast.Message"] = $"Arreglo '{arreglo.Nombre}' eliminado.";
            TempData["Toast.Type"] = "warning";

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
                return (false, null, "Solo se permiten im�genes (jpg, jpeg, png, webp, gif).");
            }

            if (archivo.Length > MAX_IMAGEN_BYTES)
            {
                return (false, null, "La imagen supera el tama�o permitido (10 MB).");
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

        private void AsegurarColoresBaseInventario(int florId)
        {
            if (florId <= 0)
            {
                return;
            }

            var existentes = context.FloresInventarioColores
                .Where(c => c.FlorId == florId)
                .Select(c => c.Color)
                .ToList();

            var setExistentes = existentes
                .Select(c => c.Trim().ToLower())
                .ToHashSet();

            var faltantes = COLORES_BASE_INVENTARIO
                .Where(color => !setExistentes.Contains(color.Trim().ToLower()))
                .Select(color => new FlorInventarioColor
                {
                    FlorId = florId,
                    Color = color,
                    Cantidad = 0
                })
                .ToList();

            if (faltantes.Count > 0)
            {
                context.FloresInventarioColores.AddRange(faltantes);
                context.SaveChanges();
            }
        }

        private Dictionary<int, List<string>> ObtenerColoresPorFlorId()
        {
            return context.FloresInventarioColores
                .AsNoTracking()
                .GroupBy(c => c.FlorId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.Color)
                          .Where(c => !string.IsNullOrWhiteSpace(c))
                          .Distinct(StringComparer.OrdinalIgnoreCase)
                          .OrderBy(c => c)
                          .ToList());
        }

        private static List<string> ObtenerOpcionesColorParaFlor(int florId, Dictionary<int, List<string>> coloresPorFlorId)
        {
            var opciones = new List<string> { "a elegir" };

            if (coloresPorFlorId.TryGetValue(florId, out var colores))
            {
                foreach (var color in colores)
                {
                    if (!opciones.Any(o => string.Equals(o, color, StringComparison.OrdinalIgnoreCase)))
                    {
                        opciones.Add(color);
                    }
                }
            }

            return opciones;
        }

        private static string NormalizarColorSeleccionado(string? color)
        {
            return string.IsNullOrWhiteSpace(color) ? "a elegir" : color.Trim();
        }

        private bool ValidarContrasenaSupervisor(string contrasenaIngresada)
        {
            var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(usuarioIdClaim, out var supervisorId))
            {
                return false;
            }

            var supervisor = context.Usuarios.Find(supervisorId);
            if (supervisor == null || supervisor.Rol != RolUsuario.supervisor)
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
    }
}
