using Blazored.LocalStorage;
using VanGest.Enums;
using VanGest.Server.Models.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VanGest.Server.Services
{
    public class ContextService : IContextService
    {
        private readonly ILocalStorageService _localStorage;
        private ContestoApplicativo _contesto = ContestoApplicativo.Prenotazione;

        public ContextService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public string LastStaffFilterFormatted
        {
            get => _lastStaffFilterFormatted;
            set
            {
                _lastStaffFilterFormatted = FormatFilterText(value);
            }
        }
        private string _lastStaffFilterFormatted = string.Empty;

        private string FormatFilterText(string filterQuery)
        {
            if (string.IsNullOrEmpty(filterQuery)) return string.Empty;

            // Decodifica URL e rimuovi HaAllarmi
            var decoded = System.Net.WebUtility.UrlDecode(filterQuery)
                .Split('&')
                .Where(param => !param.StartsWith("HaAllarmi="))  // Escludi HaAllarmi
                .Select(param => param.Replace("%24", "$"))
                .ToArray();

            return string.Join(", ", decoded);
        }







        public List<string> SelectedColumns { get; set; } = new() { "Targa", "Marca", "Comune", "Disponibile" };

        public ContestoApplicativo ContestoAttuale
        {
            get => _contesto;
            set
            {
                if (_contesto != value)
                {
                    _contesto = value;
                    OnContestoCambiato?.Invoke();
                }
            }
        }

        public event Action? OnContestoCambiato;

        public async Task LoadColumnsAsync()
        {
            try
            {
                var saved = await _localStorage.GetItemAsync<FilterState>("filtroStatoAvanzato");
                if (saved?.ColonneVisibili?.Count > 0)
                {
                    SelectedColumns = new List<string>(saved.ColonneVisibili);
                }
            }
            catch
            {
                // Fallback alle colonne di default
                SelectedColumns = new() { "Targa", "Marca", "Comune", "Disponibile" };
            }
        }

        public async Task<string?> GetLastActiveTab()
        {
            return await _localStorage.GetItemAsync<string>("lastActiveTab");
        }

        public async Task SetLastActiveTab(string tabName)
        {
            await _localStorage.SetItemAsync("lastActiveTab", tabName);
        }

    }
}