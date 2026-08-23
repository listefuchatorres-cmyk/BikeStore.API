using BikeStore.API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace BikeStore.API.Controllers
{
    public class CategoriasWebController : Controller
    {
        private readonly HttpClient _httpClient;

        public CategoriasWebController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BikeStoreAPI");
        }

        // GET: /CategoriasWeb
        public async Task<IActionResult> Index()
        {
            var categorias = await _httpClient
                .GetFromJsonAsync<List<Categoria>>(
                    "api/categorias"
                ) ?? new List<Categoria>();

            return View(categorias);
        }

        // GET: /CategoriasWeb/Crear
        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        // POST: /CategoriasWeb/Crear
        [HttpPost]
        public async Task<IActionResult> Crear(Categoria categoria)
        {
            var respuesta = await _httpClient.PostAsJsonAsync(
                "api/categorias",
                categoria
            );

            if (!respuesta.IsSuccessStatusCode)
            {
                TempData["Error"] = "No se pudo registrar la categoría.";
                return View(categoria);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /CategoriasWeb/Editar/1
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var respuesta = await _httpClient.GetAsync(
                $"api/categorias/{id}"
            );

            if (!respuesta.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var categoria = await respuesta.Content
                .ReadFromJsonAsync<Categoria>();

            if (categoria == null)
            {
                return NotFound();
            }

            return View(categoria);
        }

        // POST: /CategoriasWeb/Editar/1
        [HttpPost]
        public async Task<IActionResult> Editar(
            int id,
            Categoria categoria)
        {
            categoria.IdCategoria = id;

            var respuesta = await _httpClient.PutAsJsonAsync(
                $"api/categorias/{id}",
                categoria
            );

            if (!respuesta.IsSuccessStatusCode)
            {
                TempData["Error"] = "No se pudo actualizar la categoría.";
                return View(categoria);
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /CategoriasWeb/Eliminar/1
        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var respuesta = await _httpClient.DeleteAsync(
                $"api/categorias/{id}"
            );

            if (!respuesta.IsSuccessStatusCode)
            {
                TempData["Error"] = "No se pudo eliminar la categoría.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
