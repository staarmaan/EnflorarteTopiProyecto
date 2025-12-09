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

            return View(comandaAEditar);
        }

        public IActionResult Eliminar(int id)
        {
            var comandaAEliminar = context.Comandas.Find(id);
            if (comandaAEliminar == null)
            {
                TempData["Toast.Message"] = "Comanda no encontrada.";
                TempData["Toast.Type"] = "error";
                return RedirectToAction("Index");
            }

            context.Comandas.Remove(comandaAEliminar);
            context.SaveChanges();

            TempData["Toast.Message"] = "Comanda eliminada correctamente.";
            TempData["Toast.Type"] = "success";

            return RedirectToAction("Index");
        }
    }
}
