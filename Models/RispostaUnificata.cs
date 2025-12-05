using System.Text.Json.Serialization;
using VanGest.Server.Models.Filters;

namespace VanGest.Server.Models;

public class RispostaUnificata
{
    [JsonPropertyName("tipo")]
    public string Tipo { get; set; } = string.Empty; // "info" o "ricerca"

    [JsonPropertyName("risposta_testo")]
    public string RispostaTesto { get; set; } = string.Empty;

    [JsonPropertyName("filtri")]
    public VanFilter Filtri { get; set; } = new VanFilter();
}