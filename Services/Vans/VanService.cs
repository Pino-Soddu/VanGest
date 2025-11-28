using Microsoft.Data.SqlClient;
using System.Text;
using VanGest.Server.Models;
using VanGest.Server.Models.Filters;

namespace VanGest.Server.Services.Vans
{
    public class VanService : IVanService
    {
        private readonly string _connectionString;

        public VanService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' non trovata");
        }

        public async Task<List<Van>> GetVansAsync(VanFilter filter)
        {
            var vans = new List<Van>();
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = BuildCommand(conn, filter);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        vans.Add(MapVanFromReader(reader));
                    }
                }
            }
            return vans;
        }

        // Copia INCROLLATA dal controller
        private SqlCommand BuildCommand(SqlConnection conn, VanFilter filter)
        {
            var query = new StringBuilder(@"
                SELECT 
                    IdVeicolo, Targa, Telaio, Località, Comune, 
                    Provincia, Regione, Marca, Modello, Alimentazione,
                    Disponibile, UltUbicazione
                FROM VansView
                WHERE 1 = 1 AND Disponibile='D'");

            var parameters = new List<SqlParameter>();
            AddStringFilter(query, parameters, "Località", filter.Località);
            AddStringFilter(query, parameters, "Comune", filter.Comune);
            AddStringFilter(query, parameters, "Provincia", filter.Provincia);
            AddStringFilter(query, parameters, "Regione", filter.Regione);
            AddStringFilter(query, parameters, "Marca", filter.Marca);
            AddStringFilter(query, parameters, "Modello", filter.Modello);
            AddStringFilter(query, parameters, "Alimentazione", filter.Alimentazione);
            AddStringFilter(query, parameters, "UltUbicazione", filter.UltUbicazione);

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

        private Van MapVanFromReader(SqlDataReader reader)
        {
            return new Van
            {
                IdVeicolo = reader.GetInt32(reader.GetOrdinal("IdVeicolo")),
                Targa = reader["Targa"] as string ?? string.Empty,
                Telaio = reader["Telaio"] as string ?? string.Empty,
                Località = reader["Località"] as string ?? string.Empty,
                Comune = reader["Comune"] as string ?? string.Empty,
                Provincia = reader["Provincia"] as string ?? string.Empty,
                Regione = reader["Regione"] as string ?? string.Empty,
                Marca = reader["Marca"] as string ?? string.Empty,
                Modello = reader["Modello"] as string ?? string.Empty,
                Alimentazione = reader["Alimentazione"] as string ?? string.Empty,
                Disponibile = reader["Disponibile"] as string ?? string.Empty,
                UltUbicazione = reader["UltUbicazione"] as string ?? string.Empty,

                // Valori di default per il frontend
                ImageUrl = "/images/Vans/default.jpg",
                IncludedKm = 200,
                Deposit = 500
            };
        }
    }
}
