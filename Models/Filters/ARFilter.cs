using System.Text.Json;
using System.Text.Json.Serialization;

namespace VanGest.Server.Models.Filters;

public class ARFilter
{
    // Aggiungere questa proprietà per tracciare la selezione
    [JsonIgnore] // Escludi dalla serializzazione JSON
    public Dictionary<string, bool> CampiSelezionati { get; set; } = new();

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

    [JsonPropertyName("tipososta")]  
    public string TipoSosta { get; set; } = string.Empty;

    [JsonPropertyName("indirizzogps")] 
    public string IndirizzoGPS { get; set; } = string.Empty;

    // Aggiungi campi mancanti con lo stesso naming del modello
    [JsonPropertyName("contakm")]
    [JsonConverter(typeof(NullableIntConverter))]
    public int? ContaKm { get; set; }

    [JsonPropertyName("velocitagps")]
    [JsonConverter(typeof(NullableIntConverter))]
    public int? VelocitaGPS { get; set; }

    [JsonPropertyName("eventogps")]
    public string EventoGPS { get; set; } = string.Empty;

    [JsonPropertyName("modelloTrack")]
    public string ModelloTrack { get; set; } = string.Empty;

    [JsonPropertyName("esisteSuTrack")]
    public bool? EsisteSuTrack { get; set; }

    [JsonPropertyName("tipoSatTrack")]
    public string TipoSatTrack { get; set; } = string.Empty;

    [JsonPropertyName("matricolaTrack")]
    public string MatricolaTrack { get; set; } = string.Empty;

    [JsonPropertyName("onlineEffettivo")]
    public string OnlineEffettivo { get; set; } = "";

    [JsonPropertyName("nggOffLine")]
    [JsonConverter(typeof(NullableIntConverter))]
    public int? NggOffLine { get; set; }


    [JsonPropertyName("datascadenzapremio")]
    public DateOnly? DataScadenzaPremio { get; set; }

    [JsonPropertyName("dataprossimarevisione")]
    public DateOnly? DataProssimaRevisione { get; set; }

    [JsonPropertyName("datascadenzaatp")]
    public DateOnly? DataScadenzaATP { get; set; }

    //
    // Flag di memorizzazione delle date di attenzione
    //
    public bool HaAllarmi
    {
        get
        {
            var oggi = DateOnly.FromDateTime(DateTime.Today);
            return
                (DataScadenzaPremio.HasValue && DataScadenzaPremio < oggi) ||
                (DataProssimaRevisione.HasValue && DataProssimaRevisione < oggi) ||
                (DataScadenzaATP.HasValue && DataScadenzaATP < oggi);
        }
    }
    public bool HasFilters() =>
           !string.IsNullOrEmpty(Località)
        || !string.IsNullOrEmpty(Comune)
        || !string.IsNullOrEmpty(Provincia)
        || !string.IsNullOrEmpty(Regione)
        || !string.IsNullOrEmpty(Targa)
        || !string.IsNullOrEmpty(Telaio)
        || !string.IsNullOrEmpty(Marca)
        || !string.IsNullOrEmpty(Modello)
        || !string.IsNullOrEmpty(Alimentazione)
        || !string.IsNullOrEmpty(Segmento)
        || !string.IsNullOrEmpty(ClienteUbicazione)
        || !string.IsNullOrEmpty(Proprietario)
        || !string.IsNullOrEmpty(Locatario)
        || !string.IsNullOrEmpty(Utilizzatore)
        || !string.IsNullOrEmpty(SubUtilizzatore)
        || !string.IsNullOrEmpty(MetodoAcquisizione)
        || !string.IsNullOrEmpty(UltUbicazione)
        || !string.IsNullOrEmpty(Disponibile)
        || !string.IsNullOrEmpty(EventoGPS)
        || !string.IsNullOrEmpty(IndirizzoGPS)
        || !string.IsNullOrEmpty(ModelloTrack)
        || !string.IsNullOrEmpty(TipoSatTrack)
        || !string.IsNullOrEmpty(MatricolaTrack)
        || !string.IsNullOrEmpty(OnlineEffettivo)
        || EsisteSuTrack.HasValue
        || NggOffLine.HasValue
        || ContaKm.HasValue
        || VelocitaGPS.HasValue
        || DataScadenzaPremio.HasValue
        || DataProssimaRevisione.HasValue
        || DataScadenzaATP.HasValue
        || (IdVeicolo.HasValue && IdVeicolo > 0);

    public string ToQueryString()
    {
        var queryParams = new List<string>();

        foreach (var prop in GetType().GetProperties())
        {
            // Escludi CampiSelezionati e proprietà non filtranti
            if (prop.Name == nameof(CampiSelezionati))
                continue;

            var value = prop.GetValue(this);

            if (value != null && !string.IsNullOrEmpty(value.ToString()))
            {
                // Gestione speciale per DateOnly
                if (value is DateOnly date)
                {
                    queryParams.Add($"{prop.Name}={date:yyyy-MM-dd}");
                }
                // Filtri testuali standard
                else
                {
                    queryParams.Add($"{prop.Name}={Uri.EscapeDataString(value.ToString()!)}");
                }
            }
        }

        return string.Join("&", queryParams);
    }

    // Metodo per inizializzare i campi selezionati
    public void InizializzaSelezione()
    {
        var properties = GetType().GetProperties()
            .Where(p => p.Name != nameof(CampiSelezionati));

        foreach (var prop in properties)
        {
            if (!CampiSelezionati.ContainsKey(prop.Name))
            {
                CampiSelezionati[prop.Name] = false;
            }
        }
    }

    // Metodo per selezionare/deselezionare tutti i campi
    public void SetSelezionePerCategoria(string categoria, bool selezione)
    {
        var campiPerCategoria = new Dictionary<string, List<string>>
        {
            ["DATI BASE"] = new() { 
                nameof(IdVeicolo), 
                nameof(Targa), 
                nameof(Telaio), 
                nameof(Disponibile),
                nameof(UltUbicazione),

            },

            ["TERRITORIALE"] = new() {
                nameof(Località),
                nameof(Comune),
                nameof(Provincia),
                nameof(Regione),
            },

            ["COMMERCIALE"] = new() {
                nameof(ClienteUbicazione),
                nameof(Proprietario),
                nameof(Locatario),
                nameof(Utilizzatore),
                nameof(SubUtilizzatore),
                nameof(MetodoAcquisizione),
            },

            ["MANUTENZIONE"] = new() {
                nameof(DataScadenzaPremio),
                nameof(DataScadenzaATP),
                nameof(DataProssimaRevisione),
            },

            ["CARATTERISTICHE"] = new() {
                nameof(Marca),
                nameof(Modello),
                nameof(Alimentazione),
                nameof(Segmento),
            },

            ["TRACCIAMENTO GPS"] = new() {
                nameof(ContaKm),
                nameof(VelocitaGPS),
                nameof(EventoGPS),
                nameof(IndirizzoGPS),
                nameof(ModelloTrack),
                nameof(EsisteSuTrack),
                nameof(TipoSatTrack),
                nameof(MatricolaTrack),
                nameof(OnlineEffettivo),
                nameof(NggOffLine)
            },
        };

        if (campiPerCategoria.TryGetValue(categoria, out var campi))
        {
            foreach (var campo in campi)
            {
                CampiSelezionati[campo] = selezione;
            }
        }
    }

    public class NullableIntConverter : JsonConverter<int?>
    {
        public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (string.IsNullOrEmpty(stringValue))
                    return null;

                if (int.TryParse(stringValue, out int result))
                    return result;

                return null;
            }

            if (reader.TokenType == JsonTokenType.Number)
                return reader.GetInt32();

            return null;
        }

        public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteNumberValue(value.Value);
            else
                writer.WriteNullValue();
        }
    }

}
