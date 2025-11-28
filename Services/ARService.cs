using System.Net.Http;
using System.Net.Http.Json;
using VanGest.Server.Models;
using VanGest.Server.Models.Filters;
using VanGest.Server.Services;

namespace VanGest.Server.Services
{
    public class ARService : IARService
    {
        private readonly HttpClient _httpClient;

        public ARService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ARVan>> GetVansAsync(ARFilter filter)
        {
            try
            {
                var queryString = filter.ToQueryString();
                var response = await _httpClient.GetFromJsonAsync<List<ARVan>>($"api/AR?{queryString}");

                // Gestione esplicita del caso null
                return response ?? new List<ARVan>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Errore durante la chiamata API: {ex.StatusCode} - {ex.Message}");
                return new List<ARVan>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore inatteso: {ex.Message}");
                return new List<ARVan>();
            }
        }
        public async Task<List<ARVan>> CercaVeicoli(ARFilter filtro)
        {
            var response = await _httpClient.PostAsJsonAsync("api/staff/vans", filtro);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<ARVan>>() ?? new List<ARVan>();
        }

        public async Task<int> GetTotalCountAsync(ARFilter filter)
        {
            var queryString = filter.ToQueryString();
            return await _httpClient.GetFromJsonAsync<int>(
                $"/api/internal/ar/vans/count?{queryString}"
            );
        }
    }
}
