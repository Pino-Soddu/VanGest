using System.Collections.Generic;
using System.Threading.Tasks;
using VanGest.Server.Models.Localita;

public interface ILocalitaDataManager
{
    Task<List<Localita>> GetLocalitaAsync();
    Task<bool> DeleteLocalitaAsync(int id);
    Task<bool> UpdateLocalitaAsync(Localita item); 
}