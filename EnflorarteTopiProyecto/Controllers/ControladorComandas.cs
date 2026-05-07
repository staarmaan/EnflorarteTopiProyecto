using EnflorarteTopiProyecto.Models;
using EnflorarteTopiProyecto.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EnflorarteTopiProyecto.Controllers
{
    [Authorize]
    public class ControladorComandas : Controller
    {
        private readonly ApplicationDbContext context;

        public ControladorComandas(ApplicationDbContext context)
        {
            this.context = context;
        }

        public IActionResult Index()
        {
            var haceUnaSemana = DateTime.Now.AddDays(-7);
            var comandas = context.Comandas
                .Include(c => c.Arreglo)
                .Where(c => !c.Archivado &&
                    !(c.Liquidado && c.FechaEntrega < haceUnaSemana))
                .OrderByDescending(comanda => comanda.Id)
                .ToList();
            return View(comandas);
        }

        public IActionResult Crear()
        {
            CargarListasViewBag(); // JENNY: Cargar las listas para los usuarios y rep
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(ComandaDto comandaDto)
        {
            ValidarReglasNegocio(comandaDto); //JENNY: Validación personalizada de Lógica de Negocio

            if (!ModelState.IsValid)
            {
                CargarListasViewBag(); //JENNY: Si hay error, recargamos las listas 
                return View(comandaDto);
            }

            // Validar que el usuario creador exista
            var usuarioExiste = context.Usuarios.Any(u => u.Id == comandaDto.UsuarioId);
            if (!usuarioExiste)
            {
                ModelState.AddModelError(nameof(comandaDto.UsuarioId), "El usuario especificado no existe.");
                TempData["Toast.Message"] = "Usuario especificado no existe.";
                TempData["Toast.Type"] = "warning";
                CargarListasViewBag();
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
                    CargarListasViewBag();
                    return View(comandaDto);
                }
            }

            // Validar que el arreglo exista
            var arregloExiste = context.Arreglos.Any(a => a.Id == comandaDto.ArregloId);
            if (!arregloExiste)
            {
                ModelState.AddModelError(nameof(comandaDto.ArregloId), "El arreglo especificado no existe.");
                TempData["Toast.Message"] = "Arreglo especificado no existe.";
                TempData["Toast.Type"] = "warning";
                CargarListasViewBag();
                return View(comandaDto);
            }

            if (comandaDto.TipoArreglo == "bouquet")
            {
                comandaDto.CajaTipoArreglo = null;
            }

            ProcesarAnticipoParaGuardar(comandaDto); //JENNY: Si es porcentaje, calcula el dinero $ antes de guardar en la BD.

            // Crear nueva comanda si es valida.
            var comandaNueva = new Comanda
            {
                UsuarioId = comandaDto.UsuarioId,
                RepartidorId = comandaDto.RepartidorId,
                Estado = comandaDto.Estado,
                Liquidado = comandaDto.Liquidado,
                Archivado = comandaDto.Archivado,
                ClienteNombre = comandaDto.ClienteNombre,
                ClienteTelefono = comandaDto.ClienteTelefono,
                LinkDireccion = comandaDto.LinkDireccion,
                DomicilioReferencias = comandaDto.DomicilioReferencias,
                NumeroRuta = comandaDto.NumeroRuta,
                NumeroPedido = comandaDto.NumeroPedido,
                MedioDeLaSolicitud = comandaDto.MedioDeLaSolicitud,
                TipoEntrega = comandaDto.TipoEntrega,
                DireccionEntrega = comandaDto.DireccionEntrega,
                NombreReceptorEnvio = comandaDto.NombreReceptorEnvio,
                TelefonoReceptorEnvio = comandaDto.TelefonoReceptorEnvio,
                FechaEntrega = comandaDto.FechaEntrega,
                HoraEntrega = comandaDto.HoraEntrega,
                ArregloId = comandaDto.ArregloId,
                PrecioArreglo = comandaDto.PrecioArreglo,
                PagoEnvio = comandaDto.PagoEnvio,
                CantidadArreglo = comandaDto.CantidadArreglo,
                MensajeArreglo = comandaDto.MensajeArreglo,
                AccesorioArreglo = comandaDto.AccesorioArreglo,
                EvolturaArreglo = comandaDto.EvolturaArreglo,
                ColorEvolturaArreglo = comandaDto.ColorEvolturaArreglo,
                TipoArreglo = comandaDto.TipoArreglo,
                CajaTipoArreglo = comandaDto.CajaTipoArreglo,
                AnticipoTipo = comandaDto.AnticipoTipo,
                AnticipoPagoTotal = comandaDto.AnticipoPagoTotal
            };

            // Copiar composición del arreglo (flores y colores) a la comanda nueva.
            // Si el formulario envía `Flores`, usamos esa composición; si no, copiamos desde el Arreglo.
            var arreglo = context.Arreglos
                .Include(a => a.Flores)
                .ThenInclude(af => af.Flor)
                .FirstOrDefault(a => a.Id == comandaDto.ArregloId);

            if (arreglo != null)
            {
                comandaNueva.NombreArreglo = arreglo.Nombre;
                comandaNueva.FotoArregloRuta = arreglo.FotoRuta;

                if (comandaDto.Flores != null && comandaDto.Flores.Any())
                {
                    foreach (var f in comandaDto.Flores)
                    {
                        comandaNueva.Flores.Add(new ComandaFlor
                        {
                            FlorId = f.FlorId,
                            Cantidad = f.Cantidad,
                            ColorSeleccionado = f.ColorSeleccionado
                        });
                    }
                }
                else
                {
                    foreach (var af in arreglo.Flores)
                    {
                        comandaNueva.Flores.Add(new ComandaFlor
                        {
                            FlorId = af.FlorId,
                            Cantidad = af.Cantidad,
                            ColorSeleccionado = af.ColorSeleccionado
                        });
                    }
                }
            }

            context.Comandas.Add(comandaNueva);
            context.SaveChanges();

            TempData["Toast.Message"] = "Comanda creada correctamente.";
            TempData["Toast.Type"] = "success";

            return RedirectToAction("Index");

        }

        public IActionResult VistaPrevia(int id)
        {
            var comanda = context.Comandas
                .Include(c => c.Flores)
                .ThenInclude(cf => cf.Flor)
                .FirstOrDefault(c => c.Id == id);

            if (comanda == null)
            {
                TempData["Toast.Message"] = "Comanda no encontrada.";
                TempData["Toast.Type"] = "error";
                return RedirectToAction("Index");
            }

            CargarListasViewBag();

            var comandaDto = new ComandaDto
            {
                Id = comanda.Id,
                UsuarioId = comanda.UsuarioId,
                RepartidorId = comanda.RepartidorId,
                Estado = comanda.Estado,
                Liquidado = comanda.Liquidado,
                Archivado = comanda.Archivado,
                ClienteNombre = comanda.ClienteNombre,
                ClienteTelefono = comanda.ClienteTelefono,
                LinkDireccion = comanda.LinkDireccion,
                DomicilioReferencias = comanda.DomicilioReferencias,
                NumeroRuta = comanda.NumeroRuta,
                NumeroPedido = comanda.NumeroPedido,
                MedioDeLaSolicitud = comanda.MedioDeLaSolicitud,
                TipoEntrega = comanda.TipoEntrega,
                DireccionEntrega = comanda.DireccionEntrega,
                NombreReceptorEnvio = comanda.NombreReceptorEnvio,
                TelefonoReceptorEnvio = comanda.TelefonoReceptorEnvio,
                FechaEntrega = comanda.FechaEntrega,
                HoraEntrega = comanda.HoraEntrega,
                ArregloId = comanda.ArregloId,
                PrecioArreglo = comanda.PrecioArreglo,
                PagoEnvio = comanda.PagoEnvio,
                CantidadArreglo = comanda.CantidadArreglo,
                MensajeArreglo = comanda.MensajeArreglo,
                AccesorioArreglo = comanda.AccesorioArreglo,
                EvolturaArreglo = comanda.EvolturaArreglo,
                ColorEvolturaArreglo = comanda.ColorEvolturaArreglo,
                TipoArreglo = comanda.TipoArreglo,
                CajaTipoArreglo = comanda.CajaTipoArreglo,
                AnticipoTipo = comanda.AnticipoTipo,
                AnticipoPagoTotal = comanda.AnticipoPagoTotal,
                Flores = comanda.Flores.Select(f => new ComandaFlorDto
                {
                    FlorId = f.FlorId,
                    Nombre = f.Flor?.Nombre ?? string.Empty,
                    Cantidad = f.Cantidad,
                    ColorSeleccionado = f.ColorSeleccionado
                }).ToList()
            };

            return View(comandaDto);
        }

        [Authorize(Policy = "EsVentas")]
        public IActionResult Editar(int id)
        {
            var comandaAEditar = context.Comandas
                .Include(c => c.Flores)
                .ThenInclude(cf => cf.Flor)
                .FirstOrDefault(c => c.Id == id);

            if (comandaAEditar == null)
            {
                TempData["Toast.Message"] = "Comanda no encontrada.";
                TempData["Toast.Type"] = "error";
                return RedirectToAction("Index");
            }

            CargarListasViewBag();//JENNY: cargamos listas aqui tambien

            var comandaDto = new ComandaDto
            {
                Id = id,
                UsuarioId = comandaAEditar.UsuarioId,
                RepartidorId = comandaAEditar.RepartidorId,
                Estado = comandaAEditar.Estado,
                Liquidado = comandaAEditar.Liquidado,
                Archivado = comandaAEditar.Archivado,
                ClienteNombre = comandaAEditar.ClienteNombre,
                ClienteTelefono = comandaAEditar.ClienteTelefono,
                LinkDireccion = comandaAEditar.LinkDireccion,
                DomicilioReferencias = comandaAEditar.DomicilioReferencias,
                NumeroRuta = comandaAEditar.NumeroRuta,
                NumeroPedido = comandaAEditar.NumeroPedido,
                MedioDeLaSolicitud = comandaAEditar.MedioDeLaSolicitud,
                TipoEntrega = comandaAEditar.TipoEntrega,
                DireccionEntrega = comandaAEditar.DireccionEntrega,
                NombreReceptorEnvio = comandaAEditar.NombreReceptorEnvio,
                TelefonoReceptorEnvio = comandaAEditar.TelefonoReceptorEnvio,
                FechaEntrega = comandaAEditar.FechaEntrega,
                HoraEntrega = comandaAEditar.HoraEntrega,
                ArregloId = comandaAEditar.ArregloId,
                PrecioArreglo = comandaAEditar.PrecioArreglo,
                PagoEnvio = comandaAEditar.PagoEnvio,
                CantidadArreglo = comandaAEditar.CantidadArreglo,
                MensajeArreglo = comandaAEditar.MensajeArreglo,
                AccesorioArreglo = comandaAEditar.AccesorioArreglo,
                EvolturaArreglo = comandaAEditar.EvolturaArreglo,
                ColorEvolturaArreglo = comandaAEditar.ColorEvolturaArreglo,
                TipoArreglo = comandaAEditar.TipoArreglo,
                CajaTipoArreglo = comandaAEditar.CajaTipoArreglo,
                AnticipoTipo = comandaAEditar.AnticipoTipo,
                AnticipoPagoTotal = comandaAEditar.AnticipoPagoTotal,
                Flores = comandaAEditar.Flores.Select(f => new ComandaFlorDto
                {
                    FlorId = f.FlorId,
                    Nombre = f.Flor?.Nombre ?? string.Empty,
                    Cantidad = f.Cantidad,
                    ColorSeleccionado = f.ColorSeleccionado
                }).ToList()
            };

            ProcesarAnticipoParaVista(comandaDto); //JENNY: convierte el dinero a porcentaje para mostrarlo en la vista

            return View(comandaDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EsVentas")]
        public IActionResult Editar(int id, ComandaDto comandaDto)
        {
            var comandaExistente = context.Comandas
                .Include(c => c.Flores)
                .FirstOrDefault(c => c.Id == id);
            if (comandaExistente == null)
            {
                TempData["Toast.Message"] = "Comanda no encontrada.";
                TempData["Toast.Type"] = "error";
                return RedirectToAction("Index");
            }

            ValidarReglasNegocio(comandaDto); //JENNY: Validación personalizada de Lógica de Negocio

            if (!ModelState.IsValid)
            {
                CargarListasViewBag();
                return View(comandaDto);
            }

            // Validar que el usuario creador exista
            var usuarioExiste = context.Usuarios.Any(u => u.Id == comandaDto.UsuarioId);
            if (!usuarioExiste)
            {
                ModelState.AddModelError(nameof(comandaDto.UsuarioId), "El usuario especificado no existe.");
                TempData["Toast.Message"] = "Usuario especificado no existe.";
                TempData["Toast.Type"] = "warning";
                CargarListasViewBag();
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
                    CargarListasViewBag();
                    return View(comandaDto);
                }
            }

            // Validar que el arreglo exista
            var arregloExiste = context.Arreglos.Any(a => a.Id == comandaDto.ArregloId);
            if (!arregloExiste)
            {
                ModelState.AddModelError(nameof(comandaDto.ArregloId), "El arreglo especificado no existe.");
                TempData["Toast.Message"] = "Arreglo especificado no existe.";
                TempData["Toast.Type"] = "warning";
                CargarListasViewBag();
                return View(comandaDto);
            }

            if (comandaDto.TipoArreglo == "bouquet")
            {
                comandaDto.CajaTipoArreglo = null;
            }

                ProcesarAnticipoParaGuardar(comandaDto); //JENNY: Si cambiaron el anticipo a porcentaje, recalculamos el dinero $ antes de actualizar la BD.

            // Actualizar los campos de la comanda existente.
            var arregloCambiado = comandaExistente.ArregloId != comandaDto.ArregloId;

            comandaExistente.UsuarioId = comandaDto.UsuarioId;
            comandaExistente.RepartidorId = comandaDto.RepartidorId;
            comandaExistente.Estado = comandaDto.Estado;
            comandaExistente.Liquidado = comandaDto.Liquidado;
            comandaExistente.Archivado = comandaDto.Archivado;
            comandaExistente.ClienteNombre = comandaDto.ClienteNombre;
            comandaExistente.ClienteTelefono = comandaDto.ClienteTelefono;
            comandaExistente.LinkDireccion = comandaDto.LinkDireccion;
            comandaExistente.DomicilioReferencias = comandaDto.DomicilioReferencias;
            comandaExistente.NumeroRuta = comandaDto.NumeroRuta;
            comandaExistente.NumeroPedido = comandaDto.NumeroPedido;
            comandaExistente.MedioDeLaSolicitud = comandaDto.MedioDeLaSolicitud;
            comandaExistente.TipoEntrega = comandaDto.TipoEntrega;
            comandaExistente.DireccionEntrega = comandaDto.DireccionEntrega;
            comandaExistente.NombreReceptorEnvio = comandaDto.NombreReceptorEnvio;
            comandaExistente.TelefonoReceptorEnvio = comandaDto.TelefonoReceptorEnvio;
            comandaExistente.FechaEntrega = comandaDto.FechaEntrega;
            comandaExistente.HoraEntrega = comandaDto.HoraEntrega;
            comandaExistente.ArregloId = comandaDto.ArregloId;
            // Si el formulario envía `Flores` (con elementos), usamos esa composición enviada por el usuario.
            // Si no se envían flores, solo copiamos la composición desde el Arreglo si el usuario marcó la acción explícita `CopiarDesdeArreglo`.
            if (comandaDto.Flores != null && comandaDto.Flores.Any())
            {
                if (comandaExistente.Flores != null && comandaExistente.Flores.Any())
                {
                    context.ComandasFlores.RemoveRange(comandaExistente.Flores);
                    comandaExistente.Flores.Clear();
                }

                foreach (var f in comandaDto.Flores)
                {
                    comandaExistente.Flores.Add(new ComandaFlor
                    {
                        FlorId = f.FlorId,
                        Cantidad = f.Cantidad,
                        ColorSeleccionado = f.ColorSeleccionado
                    });
                }

                // Si además cambió el arreglo, actualizamos el snapshot de nombre/foto
                if (arregloCambiado)
                {
                    var nuevoArreglo = context.Arreglos
                        .Include(a => a.Flores)
                        .ThenInclude(af => af.Flor)
                        .FirstOrDefault(a => a.Id == comandaDto.ArregloId);

                    if (nuevoArreglo != null)
                    {
                        comandaExistente.NombreArreglo = nuevoArreglo.Nombre;
                        comandaExistente.FotoArregloRuta = nuevoArreglo.FotoRuta;
                    }
                    else
                    {
                        comandaExistente.NombreArreglo = string.Empty;
                        comandaExistente.FotoArregloRuta = null;
                    }
                }
            }
            else if (comandaDto.CopiarDesdeArreglo)
            {
                // El usuario solicitó copiar la composición desde el arreglo seleccionado.
                if (comandaExistente.Flores != null && comandaExistente.Flores.Any())
                {
                    context.ComandasFlores.RemoveRange(comandaExistente.Flores);
                    comandaExistente.Flores.Clear();
                }

                var nuevoArreglo = context.Arreglos
                    .Include(a => a.Flores)
                    .ThenInclude(af => af.Flor)
                    .FirstOrDefault(a => a.Id == comandaDto.ArregloId);

                if (nuevoArreglo != null)
                {
                    comandaExistente.NombreArreglo = nuevoArreglo.Nombre;
                    comandaExistente.FotoArregloRuta = nuevoArreglo.FotoRuta;

                    foreach (var af in nuevoArreglo.Flores)
                    {
                        comandaExistente.Flores.Add(new ComandaFlor
                        {
                            FlorId = af.FlorId,
                            Cantidad = af.Cantidad,
                            ColorSeleccionado = af.ColorSeleccionado
                        });
                    }
                }
                else
                {
                    comandaExistente.NombreArreglo = string.Empty;
                    comandaExistente.FotoArregloRuta = null;
                }
            }
            comandaExistente.PrecioArreglo = comandaDto.PrecioArreglo;
            comandaExistente.PagoEnvio = comandaDto.PagoEnvio;
            comandaExistente.CantidadArreglo = comandaDto.CantidadArreglo;
            comandaExistente.MensajeArreglo = comandaDto.MensajeArreglo;
            comandaExistente.AccesorioArreglo = comandaDto.AccesorioArreglo;
            comandaExistente.EvolturaArreglo = comandaDto.EvolturaArreglo;
            comandaExistente.ColorEvolturaArreglo = comandaDto.ColorEvolturaArreglo;
            comandaExistente.TipoArreglo = comandaDto.TipoArreglo;
            comandaExistente.CajaTipoArreglo = comandaDto.CajaTipoArreglo;
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

        public void CargarListasViewBag() //JENNY: Metodo para cargar laslistas
        {
            ViewBag.ListaUsuarios = new SelectList(context.Usuarios, "Id", "Nombre"); //Para el usuario creador

            // Trae al usuario si su rol es repartidor o si es Supervisor"
            var repartidores = context.Usuarios
                .Where(u => u.Rol == RolUsuario.repartidor || u.Rol == RolUsuario.supervisor)
                .ToList();

            ViewBag.ListaRepartidores = new SelectList(repartidores, "Id", "Nombre"); //Para los repartidores (rep y sup)
            
            // Cargar lista de arreglos disponibles (incluye composición de flores)
            var arreglos = context.Arreglos
                .Include(a => a.Flores)
                .ThenInclude(af => af.Flor)
                .ToList();

            ViewBag.ListaArreglos = new SelectList(arreglos, "Id", "Nombre");
            ViewBag.ArreglosFotoMap = arreglos.ToDictionary(a => a.Id, a => a.FotoRuta ?? string.Empty);

            // Mapa para la vista: arregloId -> lista de flores (nombre, cantidad, color)
            ViewBag.ArreglosFloresMap = arreglos.ToDictionary(
                a => a.Id,
                a => a.Flores.Select(af => new {
                    FlorId = af.FlorId,
                    Nombre = af.Flor?.Nombre ?? string.Empty,
                    Cantidad = af.Cantidad,
                    ColorSeleccionado = af.ColorSeleccionado
                }).ToList()
            );

            // Lista de flores para que el formulario de comanda permita agregar/editar flores (incluye colores disponibles)
            var flores = context.Flores
                .Include(f => f.InventarioColores)
                .OrderBy(f => f.Nombre)
                .ToList();

            ViewBag.FloresCatalogo = flores.Select(f => new
            {
                FlorId = f.Id,
                FlorNombre = f.Nombre,
                Colores = f.InventarioColores.Select(ic => ic.Color).ToList()
            }).ToList();

            ViewBag.InventarioFloresMap = flores.ToDictionary(
                f => f.Id,
                f => new
                {
                    Total = f.InventarioColores.Sum(ic => ic.Cantidad),
                    Colores = f.InventarioColores.ToDictionary(
                        ic => ic.Color ?? string.Empty,
                        ic => ic.Cantidad)
                });
        }

        private void ValidarReglasNegocio(ComandaDto dto) //JENNY: Validaciones
        {
            // Dirección obligatoria si es Envío
            if (dto.TipoEntrega == TipoEntrega.envio && string.IsNullOrWhiteSpace(dto.DireccionEntrega))
            {
                ModelState.AddModelError("DireccionEntrega", "La dirección es obligatoria para envíos a domicilio.");
            }

            if (dto.TipoEntrega == TipoEntrega.envio)
            {
                if (string.IsNullOrWhiteSpace(dto.NombreReceptorEnvio))
                {
                    ModelState.AddModelError("NombreReceptorEnvio", "El nombre del receptor es obligatorio para envíos a domicilio.");
                }

                if (!dto.TelefonoReceptorEnvio.HasValue || dto.TelefonoReceptorEnvio.Value <= 0)
                {
                    ModelState.AddModelError("TelefonoReceptorEnvio", "El teléfono del receptor es obligatorio para envíos a domicilio.");
                }
            }

            // Validación de Anticipos
            if (dto.AnticipoTipo.HasValue)
            {
                if (dto.AnticipoTipo.Value == AnticipoTipos.porcentaje)
                {
                    if (dto.AnticipoPagoTotal <= 0 || dto.AnticipoPagoTotal > 100)
                    {
                        ModelState.AddModelError("AnticipoPagoTotal", "El porcentaje debe estar entre 1% y 100%.");
                    }
                }
                else if (dto.AnticipoTipo.Value == AnticipoTipos.minimo)
                {
                    if (dto.AnticipoPagoTotal <= 0)
                    {
                        ModelState.AddModelError("AnticipoPagoTotal", "El monto del anticipo debe ser mayor a 0.");
                    }
                }
            }
        }

        // Función A: Convierte Porcentaje a dinero para la Base de Datos (Ej. el usuario pone 50% de anticipo (precio:400) pero en la BD se guarda 200)
        private void ProcesarAnticipoParaGuardar(ComandaDto dto)
        {
            if (dto.AnticipoTipo == AnticipoTipos.porcentaje && dto.PrecioArreglo > 0)
            {
                // (Precio * Porcentaje) / 100
                dto.AnticipoPagoTotal = (dto.PrecioArreglo * dto.AnticipoPagoTotal) / 100;
            }
        }

        // Función B: Convierte dinero a porcentaje para la Vista (Editar) (Para que cuando entre a editar aparezca el 50% en vez de 200)
        private void ProcesarAnticipoParaVista(ComandaDto dto)
        {
            if (dto.AnticipoTipo == AnticipoTipos.porcentaje && dto.PrecioArreglo > 0)
            {
                // (Anticipo / Precio) * 100
                decimal porcentaje = (dto.AnticipoPagoTotal / dto.PrecioArreglo) * 100;
                dto.AnticipoPagoTotal = Math.Round(porcentaje, 2); // Redondeamos para que se vea bien
            }
        }
    }
}
