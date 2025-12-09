using EnflorarteTopiProyecto.Models;
using EnflorarteTopiProyecto.Service;
using Microsoft.AspNetCore.Mvc;

namespace EnflorarteTopiProyecto.Controllers
{
    public class ControladorComandas : Controller
    {
        private readonly ApplicationDbContext context;

        public ControladorComandas(ApplicationDbContext context)
        {
            this.context = context;
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

            // Crear nueva comanda si es valida.
            var comandaNueva = new Comanda
            {
                UsuarioId = comandaDto.UsuarioId, // Todavia no se valida que exista el usuario con este id.
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
