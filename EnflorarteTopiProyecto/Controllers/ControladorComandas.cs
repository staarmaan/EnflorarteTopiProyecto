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
            var comandas = context.Comandas
                .Include(c => c.Arreglo)
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

            ProcesarAnticipoParaGuardar(comandaDto); //JENNY: Si es porcentaje, calcula el dinero $ antes de guardar en la BD.

            // Crear nueva comanda si es valida.
            var comandaNueva = new Comanda
            {
                UsuarioId = comandaDto.UsuarioId,
                RepartidorId = comandaDto.RepartidorId,
                Estado = comandaDto.Estado,
                Liquidado = comandaDto.Liquidado,
                ClienteNombre = comandaDto.ClienteNombre,
                ClienteTelefono = comandaDto.ClienteTelefono,
                LinkDireccion = comandaDto.LinkDireccion,
                DomicilioReferencias = comandaDto.DomicilioReferencias,
                NumeroRuta = comandaDto.NumeroRuta,
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
                AnticipoTipo = comandaDto.AnticipoTipo,
                AnticipoPagoTotal = comandaDto.AnticipoPagoTotal
            };

            context.Comandas.Add(comandaNueva);
            context.SaveChanges();

            TempData["Toast.Message"] = "Comanda creada correctamente.";
            TempData["Toast.Type"] = "success";

            return RedirectToAction("Index");

        }

        [Authorize(Policy = "EsVentas")]
        public IActionResult Editar(int id)
        {
            var comandaAEditar = context.Comandas.Find(id);
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
                ClienteNombre = comandaAEditar.ClienteNombre,
                ClienteTelefono = comandaAEditar.ClienteTelefono,
                LinkDireccion = comandaAEditar.LinkDireccion,
                DomicilioReferencias = comandaAEditar.DomicilioReferencias,
                NumeroRuta = comandaAEditar.NumeroRuta,
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
                AnticipoTipo = comandaAEditar.AnticipoTipo,
                AnticipoPagoTotal = comandaAEditar.AnticipoPagoTotal
            };

            ProcesarAnticipoParaVista(comandaDto); //JENNY: convierte el dinero a porcentaje para mostrarlo en la vista

            return View(comandaDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EsVentas")]
        public IActionResult Editar(int id, ComandaDto comandaDto)
        {
            var comandaExistente = context.Comandas.Find(id);
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

                ProcesarAnticipoParaGuardar(comandaDto); //JENNY: Si cambiaron el anticipo a porcentaje, recalculamos el dinero $ antes de actualizar la BD.

            // Actualizar los campos de la comanda existente.
            comandaExistente.UsuarioId = comandaDto.UsuarioId;
            comandaExistente.RepartidorId = comandaDto.RepartidorId;
            comandaExistente.Estado = comandaDto.Estado;
            comandaExistente.Liquidado = comandaDto.Liquidado;
            comandaExistente.ClienteNombre = comandaDto.ClienteNombre;
            comandaExistente.ClienteTelefono = comandaDto.ClienteTelefono;
            comandaExistente.LinkDireccion = comandaDto.LinkDireccion;
            comandaExistente.DomicilioReferencias = comandaDto.DomicilioReferencias;
            comandaExistente.NumeroRuta = comandaDto.NumeroRuta;
            comandaExistente.MedioDeLaSolicitud = comandaDto.MedioDeLaSolicitud;
            comandaExistente.TipoEntrega = comandaDto.TipoEntrega;
            comandaExistente.DireccionEntrega = comandaDto.DireccionEntrega;
            comandaExistente.NombreReceptorEnvio = comandaDto.NombreReceptorEnvio;
            comandaExistente.TelefonoReceptorEnvio = comandaDto.TelefonoReceptorEnvio;
            comandaExistente.FechaEntrega = comandaDto.FechaEntrega;
            comandaExistente.HoraEntrega = comandaDto.HoraEntrega;
            comandaExistente.ArregloId = comandaDto.ArregloId;
            comandaExistente.PrecioArreglo = comandaDto.PrecioArreglo;
            comandaExistente.PagoEnvio = comandaDto.PagoEnvio;
            comandaExistente.CantidadArreglo = comandaDto.CantidadArreglo;
            comandaExistente.MensajeArreglo = comandaDto.MensajeArreglo;
            comandaExistente.AccesorioArreglo = comandaDto.AccesorioArreglo;
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
            
            // Cargar lista de arreglos disponibles
            ViewBag.ListaArreglos = new SelectList(context.Arreglos, "Id", "Nombre");
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
