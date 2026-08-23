using BikeStore.API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace BikeStore.API.Controllers
{
    [Route("DetalleVentasWeb")]
    public class DetalleVentasWebController : Controller
    {
        private readonly HttpClient _httpClient;

        public DetalleVentasWebController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BikeStoreAPI");
        }

        // GET: /DetalleVentasWeb
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var detalles = await _httpClient
                .GetFromJsonAsync<List<DetalleVenta>>(
                    "api/detalleventas"
                ) ?? new List<DetalleVenta>();

            return View(detalles);
        }

        // GET: /DetalleVentasWeb/Crear
        [HttpGet("Crear")]
        public async Task<IActionResult> Crear()
        {
            await CargarDatos();

            return View();
        }

        // POST: /DetalleVentasWeb/Crear
        [HttpPost("Crear")]
        public async Task<IActionResult> Crear(DetalleVenta detalle)
        {
            var respuesta = await _httpClient.PostAsJsonAsync(
                "api/detalleventas",
                detalle
            );

            if (!respuesta.IsSuccessStatusCode)
            {
                var mensaje = await respuesta.Content.ReadAsStringAsync();

                TempData["Error"] =
                    "No se pudo registrar el detalle. " + mensaje;

                await CargarDatos();

                return View(detalle);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /DetalleVentasWeb/Editar/1
        [HttpGet("Editar/{id}")]
        public async Task<IActionResult> Editar(int id)
        {
            var respuesta = await _httpClient.GetAsync(
                $"api/detalleventas/{id}"
            );

            if (!respuesta.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var detalle = await respuesta.Content
                .ReadFromJsonAsync<DetalleVenta>();

            if (detalle == null)
            {
                return NotFound();
            }

            await CargarDatos();

            return View(detalle);
        }

        // POST: /DetalleVentasWeb/Editar/1
        [HttpPost("Editar/{id}")]
        public async Task<IActionResult> Editar(
            int id,
            DetalleVenta detalle)
        {
            detalle.IdDetalle = id;

            var respuesta = await _httpClient.PutAsJsonAsync(
                $"api/detalleventas/{id}",
                detalle
            );

            if (!respuesta.IsSuccessStatusCode)
            {
                var mensaje = await respuesta.Content.ReadAsStringAsync();

                TempData["Error"] =
                    "No se pudo actualizar el detalle. " + mensaje;

                await CargarDatos();

                return View(detalle);
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /DetalleVentasWeb/Eliminar/1
        [HttpPost("Eliminar/{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var respuesta = await _httpClient.DeleteAsync(
                $"api/detalleventas/{id}"
            );

            if (!respuesta.IsSuccessStatusCode)
            {
                var mensaje = await respuesta.Content.ReadAsStringAsync();

                TempData["Error"] =
                    "No se pudo eliminar el detalle. " + mensaje;

                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        // Cargar ventas y bicicletas para los formularios
        private async Task CargarDatos()
        {
            var ventas = await _httpClient
                .GetFromJsonAsync<List<Venta>>(
                    "api/ventas"
                ) ?? new List<Venta>();

            var bicicletas = await _httpClient
                .GetFromJsonAsync<List<BicicletaViewModel>>(
                    "api/bicicletas"
                ) ?? new List<BicicletaViewModel>();

            ViewBag.Ventas = ventas;
            ViewBag.Bicicletas = bicicletas;
        }
    }
}