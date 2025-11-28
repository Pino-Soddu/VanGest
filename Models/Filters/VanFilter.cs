
using System.Text.Json.Serialization;

namespace VanGest.Server.Models.Filters;

public class VanFilter
{
    [JsonPropertyName("idveicolo")]
    public int? IdVeicolo { get; set; }

    [JsonPropertyName("targa")]
    public string Targa { get; set; } = string.Empty;

    [JsonPropertyName("telaio")]
    public string Telaio { get; set; } = string.Empty;

    [JsonPropertyName("località")] // Manteniamo lowercase come da backend
    public string Località { get; set; } = string.Empty;

    [JsonPropertyName("comune")]
    public string Comune { get; set; } = string.Empty;

    [JsonPropertyName("provincia")]
    public string Provincia { get; set; } = string.Empty;

    [JsonPropertyName("regione")]
    public string Regione { get; set; } = string.Empty;

    [JsonPropertyName("marca")]
    public string Marca { get; set; } = string.Empty;

    [JsonPropertyName("modello")]
    public string Modello { get; set; } = string.Empty;

    [JsonPropertyName("alimentazione")]
    public string Alimentazione { get; set; } = string.Empty;

    [JsonPropertyName("disponibile")]
    public string Disponibile { get; set; } = string.Empty;

    [JsonPropertyName("ultUbicazione")]
    public string UltUbicazione { get; set; } = string.Empty;

    public bool HasFilters() => !string.IsNullOrEmpty(Località)
        || !string.IsNullOrEmpty(Comune)
        || !string.IsNullOrEmpty(Provincia)
        || !string.IsNullOrEmpty(Regione)
        || !string.IsNullOrEmpty(Marca)
        || !string.IsNullOrEmpty(Modello)
        || !string.IsNullOrEmpty(Alimentazione)
        || !string.IsNullOrEmpty(Targa)
        || !string.IsNullOrEmpty(Telaio)
        || !string.IsNullOrEmpty(UltUbicazione)
        || !string.IsNullOrEmpty(Disponibile)
        || IdVeicolo>0;

    public string ToQueryString()
    {
        var properties = GetType().GetProperties();
        var queryParams = new List<string>();

        foreach (var prop in properties)
        {
            var value = prop.GetValue(this);
            if (value != null)
            {
                queryParams.Add($"{prop.Name}={Uri.EscapeDataString(value.ToString()!)}");
            }
        }

        return string.Join("&", queryParams);
    }
}
