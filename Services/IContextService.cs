using VanGest.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VanGest.Server.Services
{
    public interface IContextService
    {
        Task LoadColumnsAsync();
        string LastStaffFilterFormatted { get; set; }
        List<string> SelectedColumns { get; set; }
        Task<string?> GetLastActiveTab();
        Task SetLastActiveTab(string tabName);
        ContestoApplicativo ContestoAttuale { get; set; }  // Aggiungi 'set'
        event Action OnContestoCambiato;
    }
}
