using System.Text.Json.Serialization;

namespace VanGest.Server.Models
{
    public class Van
    {
        [JsonPropertyName("idVeicolo")]
        public int IdVeicolo { get; set; }

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

        [JsonPropertyName("Disponibile")]
        public string Disponibile { get; set; } = string.Empty;

        [JsonPropertyName("ultUbicazione")]
        public string UltUbicazione { get; set; } = string.Empty;

        // Campi aggiuntivi frontend (OK)
        public string ImageUrl { get; set; } = "/images/Vans/default.jpg";
        public int IncludedKm { get; set; } = 200;
        public decimal Deposit { get; set; } = 500;

    }
}