using BikeStore.API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace BikeStore.API.Controllers
{
    [Route("VentasWeb")]
    public class VentasWebController : Controller
    {
        private readonly HttpClient _httpClient;

        public VentasWebController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BikeStoreAPI");
        }

        // GET: /VentasWeb
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var ventas = await _httpClient
                .GetFromJsonAsync<List<Venta>>(
                    "api/ventas"
                ) ?? new List<Venta>();

            var clientes = await _httpClient
                .GetFromJsonAsync<List<Cliente>>(
                    "api/clientes"
                ) ?? new List<Cliente>();

            ViewBag.Clientes = clientes;

            return View(ventas);
        }

        // GET: /VentasWeb/Crear
        [HttpGet("Crear")]
        public async Task<IActionResult> Crear()
        {
            await CargarClientes();

            return View();
        }

        // POST: /VentasWeb/Crear
        [HttpPost("Crear")]
        public async Task<IActionResult> Crear(Venta venta)
        {
            var respuesta = await _httpClient.PostAsJsonAsync(
                "api/ventas",
                venta
            );

            if (!respuesta.IsSuccessStatusCode)
            {
                var detalle = await respuesta.Content.ReadAsStringAsync();

                TempData["Error"] =
                    "No se pudo registrar la venta. " + detalle;

                await CargarClientes();

                return View(venta);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /VentasWeb/Editar/1
        [HttpGet("Editar/{id}")]
        public async Task<IActionResult> Editar(int id)
        {
            var respuesta = await _httpClient.GetAsync(
                $"api/ventas/{id}"
            );

            if (!respuesta.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var venta = await respuesta.Content
                .ReadFromJsonAsync<Venta>();

            if (venta == null)
            {
                return NotFound();
            }

            await CargarClientes();

            return View(venta);
        }

        // POST: /VentasWeb/Editar/1
        [HttpPost("Editar/{id}")]
        public async Task<IActionResult> Editar(
            int id,
            Venta venta)
        {
            venta.IdVenta = id;

            var respuesta = await _httpClient.PutAsJsonAsync(
                $"api/ventas/{id}",
                venta
            );

            if (!respuesta.IsSuccessStatusCode)
            {
                var detalle = await respuesta.Content.ReadAsStringAsync();

                TempData["Error"] =
                    "No se pudo actualizar la venta. " + detalle;

                await CargarClientes();

                return View(venta);
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /VentasWeb/Eliminar/1
        [HttpPost("Eliminar/{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var respuesta = await _httpClient.DeleteAsync(
                $"api/ventas/{id}"
            );

            if (!respuesta.IsSuccessStatusCode)
            {
                var detalle = await respuesta.Content.ReadAsStringAsync();

                TempData["Error"] =
                    "No se pudo eliminar la venta. " + detalle;

                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        // Cargar clientes para los formularios
        private async Task CargarClientes()
        {
            var clientes = await _httpClient
                .GetFromJsonAsync<List<Cliente>>(
                    "api/clientes"
                ) ?? new List<Cliente>();

            ViewBag.Clientes = clientes;
        }
    }
}