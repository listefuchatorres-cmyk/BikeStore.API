using BikeStore.API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace BikeStore.API.Controllers
{
    [Route("ClientesWeb")]
    public class ClientesWebController : Controller
    {
        private readonly HttpClient _httpClient;

        public ClientesWebController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BikeStoreAPI");
        }

        // GET: /ClientesWeb
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var clientes = await _httpClient
                .GetFromJsonAsync<List<Cliente>>(
                    "api/clientes"
                ) ?? new List<Cliente>();

            return View(clientes);
        }

        // GET: /ClientesWeb/Buscar
        [HttpGet("Buscar")]
        public async Task<IActionResult> Buscar(string tipo, string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return RedirectToAction(nameof(Index));
            }

            List<Cliente> clientes;

            if (tipo == "cedula")
            {
                var respuesta = await _httpClient.GetAsync(
                    $"api/clientes/buscar/cedula/{Uri.EscapeDataString(valor.Trim())}"
                );

                if (!respuesta.IsSuccessStatusCode)
                {
                    TempData["Error"] = "No se encontró ningún cliente con esa cédula.";
                    return View("Index", new List<Cliente>());
                }

                var cliente = await respuesta.Content
                    .ReadFromJsonAsync<Cliente>();

                clientes = cliente != null
                    ? new List<Cliente> { cliente }
                    : new List<Cliente>();
            }
            else
            {
                var respuesta = await _httpClient.GetAsync(
                    $"api/clientes/buscar/apellido/{Uri.EscapeDataString(valor.Trim())}"
                );

                if (!respuesta.IsSuccessStatusCode)
                {
                    TempData["Error"] = "No se encontraron clientes con ese apellido.";
                    return View("Index", new List<Cliente>());
                }

                clientes = await respuesta.Content
                    .ReadFromJsonAsync<List<Cliente>>()
                    ?? new List<Cliente>();
            }

            return View("Index", clientes);
        }

        // GET: /ClientesWeb/Crear
        [HttpGet("Crear")]
        public IActionResult Crear()
        {
            return View();
        }

        // POST: /ClientesWeb/Crear
        [HttpPost("Crear")]
        public async Task<IActionResult> Crear(Cliente cliente)
        {
            var respuesta = await _httpClient.PostAsJsonAsync(
                "api/clientes",
                cliente
            );

            if (!respuesta.IsSuccessStatusCode)
            {
                var detalle = await respuesta.Content.ReadAsStringAsync();

                TempData["Error"] =
                    "No se pudo registrar el cliente. " + detalle;

                return View(cliente);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /ClientesWeb/Editar/1
        [HttpGet("Editar/{id}")]
        public async Task<IActionResult> Editar(int id)
        {
            var respuesta = await _httpClient.GetAsync(
                $"api/clientes/{id}"
            );

            if (!respuesta.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var cliente = await respuesta.Content
                .ReadFromJsonAsync<Cliente>();

            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // POST: /ClientesWeb/Editar/1
        [HttpPost("Editar/{id}")]
        public async Task<IActionResult> Editar(
            int id,
            Cliente cliente)
        {
            cliente.IdCliente = id;

            var respuesta = await _httpClient.PutAsJsonAsync(
                $"api/clientes/{id}",
                cliente
            );

            if (!respuesta.IsSuccessStatusCode)
            {
                var detalle = await respuesta.Content.ReadAsStringAsync();

                TempData["Error"] =
                    "No se pudo actualizar el cliente. " + detalle;

                return View(cliente);
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /ClientesWeb/Eliminar/1
        [HttpPost("Eliminar/{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var respuesta = await _httpClient.DeleteAsync(
                $"api/clientes/{id}"
            );

            if (!respuesta.IsSuccessStatusCode)
            {
                var detalle = await respuesta.Content.ReadAsStringAsync();

                TempData["Error"] =
                    "No se pudo eliminar el cliente. " + detalle;

                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
