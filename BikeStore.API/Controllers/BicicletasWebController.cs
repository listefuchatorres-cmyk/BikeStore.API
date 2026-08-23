using BikeStore.API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace BikeStore.API.Controllers
{
    public class BicicletasWebController : Controller
    {
        private readonly HttpClient _httpClient;

        public BicicletasWebController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BikeStoreAPI");
        }

        public async Task<IActionResult> Index(
            string? marca,
            string? modelo,
            int? categoria,
            string? stock)
        {
            var bicicletas = await _httpClient
                .GetFromJsonAsync<List<BicicletaViewModel>>(
                    "api/bicicletas"
                ) ?? new List<BicicletaViewModel>();

            // Filtrar por marca
            if (!string.IsNullOrWhiteSpace(marca))
            {
                bicicletas = bicicletas
                    .Where(b => b.Marca.Contains(
                        marca,
                        StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Filtrar por modelo
            if (!string.IsNullOrWhiteSpace(modelo))
            {
                bicicletas = bicicletas
                    .Where(b => b.Modelo.Contains(
                        modelo,
                        StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Filtrar por categoría
            if (categoria.HasValue)
            {
                bicicletas = bicicletas
                    .Where(b => b.IdCategoria == categoria.Value)
                    .ToList();
            }

            // Filtrar por categoría
            if (categoria.HasValue)
            {
                bicicletas = bicicletas
                    .Where(b => b.IdCategoria == categoria.Value)
                    .ToList();
            }

            // Filtrar bicicletas con stock bajo
            if (stock == "bajo")
            {
                bicicletas = bicicletas
                    .Where(b => b.Stock > 0 && b.Stock <= 5)
                    .ToList();
            }

            // Filtrar bicicletas agotadas
            if (stock == "agotado")
            {
                bicicletas = bicicletas
                    .Where(b => b.Stock == 0)
                    .ToList();
            }


            return View(bicicletas);
        }

        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(BicicletaViewModel bicicleta)
        {
            var respuesta = await _httpClient.PostAsJsonAsync(
                "api/bicicletas",
                bicicleta
            );

            if (respuesta.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            var mensaje = await respuesta.Content.ReadAsStringAsync();

            ViewBag.Error = "No se pudo registrar la bicicleta: " + mensaje;

            return View(bicicleta);
        }

        public async Task<IActionResult> Editar(int id)
        {
            var bicicleta = await _httpClient
                .GetFromJsonAsync<BicicletaViewModel>(
                    $"api/bicicletas/{id}"
                );

            if (bicicleta == null)
            {
                return NotFound();
            }

            return View(bicicleta);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(
            int id,
            BicicletaViewModel bicicleta)
        {
            var respuesta = await _httpClient.PutAsJsonAsync(
                $"api/bicicletas/{id}",
                bicicleta
            );

            if (respuesta.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            var mensaje = await respuesta.Content.ReadAsStringAsync();

            ViewBag.Error = "No se pudo actualizar la bicicleta: " + mensaje;

            return View(bicicleta);


        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var respuesta = await _httpClient.DeleteAsync(
                $"api/bicicletas/{id}"
            );

            if (respuesta.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            var mensaje = await respuesta.Content.ReadAsStringAsync();

            TempData["Error"] = "No se pudo eliminar la bicicleta: " + mensaje;

            return RedirectToAction(nameof(Index));
        }
    }
}
