using System.Text.Json.Serialization;

namespace VanGest.Server.Models
{
    public class ARVan
    {
        [JsonPropertyName("idveicolo")]
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

        [JsonPropertyName("clienteubicazione")]
        public string ClienteUbicazione { get; set; } = string.Empty;

        [JsonPropertyName("proprietario")]
        public string Proprietario { get; set; } = string.Empty;

        [JsonPropertyName("locatario")]
        public string Locatario { get; set; } = string.Empty;

        [JsonPropertyName("utilizzatore")]
        public string Utilizzatore { get; set; } = string.Empty;

        [JsonPropertyName("subutilizzatore")]
        public string SubUtilizzatore { get; set; } = string.Empty;

        [JsonPropertyName("metodoacquisizione")]
        public string MetodoAcquisizione { get; set; } = string.Empty;

        [JsonPropertyName("ultUbicazione")]
        public string UltUbicazione { get; set; } = string.Empty;

        [JsonPropertyName("marca")]
        public string Marca { get; set; } = string.Empty;

        [JsonPropertyName("modello")]
        public string Modello { get; set; } = string.Empty;

        [JsonPropertyName("alimentazione")]
        public string Alimentazione { get; set; } = string.Empty;

        [JsonPropertyName("segmento")]
        public string Segmento { get; set; } = string.Empty;

        [JsonPropertyName("disponibile")]
        public string Disponibile { get; set; } = string.Empty;

        [JsonPropertyName("indirizzogps")]
        public string IndirizzoGPS { get; set; } = string.Empty;

        [JsonPropertyName("contakm")]
        public int? ContaKm { get; set; }

        [JsonPropertyName("velocitagps")]
        public int? VelocitaGPS { get; set; } 

        [JsonPropertyName("eventogps")]
        public string EventoGPS { get; set; } = string.Empty;

        [JsonPropertyName("tipososta")]
        public string? TipoSosta { get; set; } // "M", "F", "S", "P"

        [JsonPropertyName("datainizio")]
        public DateTime? DataInizio { get; set; } // Obbligatorio per tutti gli stati

        [JsonPropertyName("datafine")]
        public DateTime? DataFine { get; set; } // Valorizzato solo per F/S/P

        [JsonPropertyName("latitudine")]
        public decimal Latitudine { get; set; } = 0;

        [JsonPropertyName("longitudine")]
        public decimal Longitudine { get; set; } = 0;

        [JsonPropertyName("modellotrack")]
        public string ModelloTrack { get; set; } = string.Empty;

        [JsonPropertyName("esistesutrack")]
        public bool EsisteSuTrack { get; set; }

        [JsonPropertyName("tiposattrack")]
        public string TipoSatTrack { get; set; } = string.Empty;

        [JsonPropertyName("matricolatrack")]
        public string MatricolaTrack { get; set; } = string.Empty;

        [JsonPropertyName("onlineeffettivo")]
        public string OnlineEffettivo { get; set; } = string.Empty;

        [JsonPropertyName("nggoffLine")]
        public int? NggOffLine { get; set; }


        [JsonPropertyName("datascadenzapremio")]
        public DateOnly? DataScadenzaPremio { get; set; }

        [JsonPropertyName("dataprossimarevisione")]
        public DateOnly? DataProssimaRevisione { get; set; }

        [JsonPropertyName("datascadenzaatp")]
        public DateOnly? DataScadenzaATP { get; set; }

        public bool HaAllarmi
        {
            get
            {
                var oggi = DateOnly.FromDateTime(DateTime.Today);
                return
                    (DataScadenzaPremio < oggi) ||
                    (DataProssimaRevisione < oggi) ||
                    (DataScadenzaATP < oggi);
            }
        }
    }
}