// Services/ARVans/ARVanService.cs
using DocumentFormat.OpenXml.Office.Word;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Text;
using VanGest.Server.Models;
using VanGest.Server.Models.Filters;

namespace VanGest.Server.Services.ARVans
{
    public class ARVanService : IARVanService
    {
        private readonly string _connectionString;

        public ARVanService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' non trovata");
        }

        public async Task<List<ARVan>> GetVansAsync(ARFilter filter)
        {
            var ARvans = new List<ARVan>();
            try
            {
                //Console.WriteLine($"=== FILTRO RICEVUTO ===");
                //Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(filter));
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    var cmd = BuildCommand(conn, filter);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            ARvans.Add(MapARVanFromReader(reader));
                        }
                    }
                }

                return ARvans;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore: {ex.Message}");
                return ARvans;
            }
        }

        private SqlCommand BuildCommand(SqlConnection conn, ARFilter filter)
        {
            var query = new StringBuilder(@"
            SELECT 
                IdVeicolo, Targa, Telaio, Località, Comune, Provincia, Regione, 
                ClienteUbicazione, Proprietario, Locatario, Utilizzatore, SubUtilizzatore, 
                MetodoAcquisizione, UltUbicazione, Disponibile,
                Marca, Modello, Alimentazione, Segmento,  
                TipoSosta, DataInizio, DataFine,
                ContaKmFine, IndirizzoUltimaRilevazioneGPS,
                VelocitaGPS, EventoGPS, Latitudine, Longitudine,
                DataScadenzaPremio, DataProssimaRevisione, DataScadenzaATP,
                ModelloTrack, EsisteSuTrack, TipoSatTrack, MatricolaTrack, OnlineEffettivo, NggOffLine
            FROM VansView
            WHERE 1 = 1");

            var parameters = new List<SqlParameter>();

            // Filtri territoriali
            AddStringFilter(query, parameters, "Località", filter.Località);
            AddStringFilter(query, parameters, "Comune", filter.Comune);
            AddStringFilter(query, parameters, "Provincia", filter.Provincia);
            AddStringFilter(query, parameters, "Regione", filter.Regione);

            // Filtri veicolo
            AddStringFilter(query, parameters, "Targa", filter.Targa);
            AddStringFilter(query, parameters, "Telaio", filter.Telaio);
            AddStringFilter(query, parameters, "Marca", filter.Marca);
            AddStringFilter(query, parameters, "Modello", filter.Modello);
            AddStringFilter(query, parameters, "Alimentazione", filter.Alimentazione);
            AddStringFilter(query, parameters, "Segmento", filter.Segmento);

            // Filtri Commerciali

            AddStringFilter(query, parameters, "ClienteUbicazione", filter.ClienteUbicazione);
            AddStringFilter(query, parameters, "Proprietario", filter.Proprietario);
            AddStringFilter(query, parameters, "Locatario", filter.Locatario);
            AddStringFilter(query, parameters, "Utilizzatore", filter.Utilizzatore);
            AddStringFilter(query, parameters, "SubUtilizzatore", filter.SubUtilizzatore);
            AddStringFilter(query, parameters, "MetodoAcquisizione", filter.MetodoAcquisizione);
            AddStringFilter(query, parameters, "UltUbicazione", filter.UltUbicazione);

            // Filtri Manutenzione
            AddDateFilter(query, parameters, "DataScadenzaPremio", filter.DataScadenzaPremio);
            AddDateFilter(query, parameters, "DataProssimaRevisione", filter.DataProssimaRevisione);
            AddDateFilter(query, parameters, "DataScadenzaATP", filter.DataScadenzaATP);

            // Filtri Tracciamento GPS
            AddNumericFilter(query, parameters, "ContaKmFine", filter.ContaKm);
            AddNumericFilter(query, parameters, "VelocitaGPS", filter.VelocitaGPS);
            AddStringFilter(query, parameters, "ModelloTrack", filter.ModelloTrack);

            AddStringFilter(query, parameters, "TipoSatTrack", filter.TipoSatTrack);
            AddStringFilter(query, parameters, "MatricolaTrack", filter.MatricolaTrack);
            AddNumericFilter(query, parameters, "NggOffLine", filter.NggOffLine);
            //
            // filtro per EsisteSuTrack (Filtra SOLO se esplicitamente true)
            //
            if (filter.EsisteSuTrack == true) 
            {
                query.Append(" AND EsisteSuTrack = 1");
            }
            //
            // filtro per OnlineEffettivo (valori: "SI", "NO",  o null per entrambi)
            //
            if (!string.IsNullOrEmpty(filter.OnlineEffettivo))
            {
                query.Append(" AND OnlineEffettivo = @OnlineEffettivo");
                parameters.Add(new SqlParameter("@OnlineEffettivo", filter.OnlineEffettivo));
            }
            //
            // filtro per TipoSosta (valori: "M", "F", "S", "P" o null per tutti)
            //
            if (!string.IsNullOrEmpty(filter.TipoSosta))
            {
                query.Append(" AND TipoSosta = @TipoSosta");
                parameters.Add(new SqlParameter("@TipoSosta", filter.TipoSosta));
            }
            // Filtri speciali
            if (!string.IsNullOrEmpty(filter.Disponibile))
            {
                query.Append(" AND Disponibile = @Disponibile");
                parameters.Add(new SqlParameter("@Disponibile", filter.Disponibile));
            }

            if (filter.IdVeicolo.HasValue && filter.IdVeicolo > 0)
            {
                query.Append(" AND IdVeicolo = @IdVeicolo");
                parameters.Add(new SqlParameter("@IdVeicolo", filter.IdVeicolo.Value));
            }

            query.Append(" ORDER BY Località, DataScadenzaPremio");

            //Console.WriteLine($"=== QUERY GENERATA ===");
            //Console.WriteLine(query);

            var cmd = new SqlCommand(query.ToString(), conn);
            cmd.Parameters.AddRange(parameters.ToArray());
            return cmd;
        }

        private void AddStringFilter(StringBuilder query, List<SqlParameter> parameters, string fieldName, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                query.Append($" AND {fieldName} COLLATE SQL_Latin1_General_CP1_CI_AS LIKE @{fieldName}");
                parameters.Add(new SqlParameter($"@{fieldName}", $"%{value}%"));
            }
        }

        private void AddDateFilter(StringBuilder query, List<SqlParameter> parameters, string fieldName, DateOnly? value)
        {
            if (value.HasValue)
            {
                query.Append($" AND {fieldName} = @{fieldName}");
                parameters.Add(new SqlParameter($"@{fieldName}", value.Value.ToString("yyyy-MM-dd")));
            }
        }

        private void AddNumericFilter(StringBuilder query, List<SqlParameter> parameters, string fieldName, int? value)
        {
            if (value.HasValue && value > 0)  // Filtra solo se > 0
            {
                query.Append($" AND {fieldName} >= @{fieldName}");
                parameters.Add(new SqlParameter($"@{fieldName}", value.Value));
            }
        }

        private void AddBooleanFilter(StringBuilder query, List<SqlParameter> parameters, string fieldName, bool? value)
        {
            if (value.HasValue)
            {
                query.Append($" AND {fieldName} = @{fieldName}");
                parameters.Add(new SqlParameter($"@{fieldName}", value.Value));
            }
        }

        private ARVan MapARVanFromReader(SqlDataReader reader)
        {
            return new ARVan
            {
                IdVeicolo = reader.GetInt32(reader.GetOrdinal("IdVeicolo")),
                Targa = reader["Targa"] as string ?? string.Empty,
                Telaio = reader["Telaio"] as string ?? string.Empty,
                Località = reader["Località"] as string ?? string.Empty,
                Comune = reader["Comune"] as string ?? string.Empty,
                Provincia = reader["Provincia"] as string ?? string.Empty,
                Regione = reader["Regione"] as string ?? string.Empty,

                ClienteUbicazione = reader["ClienteUbicazione"] as string ?? string.Empty,
                Proprietario = reader["Proprietario"] as string ?? string.Empty,
                Locatario = reader["Locatario"] as string ?? string.Empty,
                Utilizzatore = reader["Utilizzatore"] as string ?? string.Empty,
                SubUtilizzatore = reader["SubUtilizzatore"] as string ?? string.Empty,
                MetodoAcquisizione = reader["MetodoAcquisizione"] as string ?? string.Empty,
                UltUbicazione = reader["UltUbicazione"] as string ?? string.Empty,

                Marca = reader["Marca"] as string ?? string.Empty,
                Modello = reader["Modello"] as string ?? string.Empty,
                Alimentazione = reader["Alimentazione"] as string ?? string.Empty,
                Segmento = reader["Segmento"] as string ?? string.Empty,
                Disponibile = reader["Disponibile"] as string ?? string.Empty,
                EventoGPS = reader["EventoGPS"] as string ?? string.Empty,
                IndirizzoGPS = reader.IsDBNull("IndirizzoUltimaRilevazioneGPS") ? "" : reader.GetString("IndirizzoUltimaRilevazioneGPS"),
                ContaKm = reader.IsDBNull("ContaKmFine") ? 0 : reader.GetInt32("ContaKmFine"),
                VelocitaGPS = reader.IsDBNull("VelocitaGPS") ? 0 : reader.GetInt32("VelocitaGPS"),
                Latitudine = reader["Latitudine"] is DBNull ?
                    0 : Convert.ToDecimal(reader["Latitudine"]),

                Longitudine = reader["Longitudine"] is DBNull ?
                    0 : Convert.ToDecimal(reader["Longitudine"]),

                TipoSosta = reader["TipoSosta"] as string,
                DataInizio = reader.IsDBNull("DataInizio") ? null : reader.GetDateTime("DataInizio"),
                DataFine = reader.IsDBNull("DataFine") ? null : reader.GetDateTime("DataFine"),
                ModelloTrack = reader["ModelloTrack"] as string ?? string.Empty,
                EsisteSuTrack = reader["EsisteSuTrack"] as bool? ?? false,
                TipoSatTrack = reader["TipoSatTrack"] as string ?? string.Empty,
                MatricolaTrack = reader["MatricolaTrack"] as string ?? string.Empty,
                OnlineEffettivo = reader["OnlineEffettivo"] as string ?? string.Empty,
                NggOffLine = reader.IsDBNull("NggOffLine") ? null : reader.GetInt32("NggOffLine"),


                DataScadenzaPremio = reader["DataScadenzaPremio"] is DBNull ?
                    null :
                    DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("DataScadenzaPremio"))),

                DataProssimaRevisione = reader["DataProssimaRevisione"] is DBNull ?
                    null :
                    DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("DataProssimaRevisione"))),

                DataScadenzaATP = reader["DataScadenzaATP"] is DBNull ?
                    null :
                    DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("DataScadenzaATP")))
            };
        }
    }
}