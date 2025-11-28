// Services/ARVans/IARVanService.cs
using VanGest.Server.Models;
using VanGest.Server.Models.Filters;

namespace VanGest.Server.Services.ARVans
{
    public interface IARVanService
    {
        Task<List<ARVan>> GetVansAsync(ARFilter filter);
    }
}