using VanGest.Server.Models;
using VanGest.Server.Models.Filters;

namespace VanGest.Server.Services
{
    public interface IARService
    {
        Task<List<ARVan>> GetVansAsync(ARFilter filter);
        Task<int> GetTotalCountAsync(ARFilter filter);
        Task<List<ARVan>> CercaVeicoli(ARFilter filtro);
    }
}