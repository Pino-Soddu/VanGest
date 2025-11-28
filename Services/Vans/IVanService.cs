using VanGest.Server.Models;
using VanGest.Server.Models.Filters;

namespace VanGest.Server.Services.Vans
{
    public interface IVanService
    {
        Task<List<Van>> GetVansAsync(VanFilter filter);
    }
}