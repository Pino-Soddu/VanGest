using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using VanGest.Server.Models.Localita;

public class LocalitaDataManager : ILocalitaDataManager
{
    private readonly string _connectionString;

    public LocalitaDataManager(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' non trovata");
    }

    public async Task<List<Localita>> GetLocalitaAsync()
    {
        var lista = new List<Localita>();
        try
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var cmd = new SqlCommand(@"
                    SELECT 
                        IdLocalita,
                        NomeLocalita,
                        Indirizzo,
                        Comune,
                        IdComune,
                        Latitudine,
                        Longitudine,
                        NomeResponsabile,
                        TelefonoResponsabile,
                        EmailResponsabile,
                        Note
                    FROM Localita", conn);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new Localita
                        {
                            IdLocalita = reader.GetInt32(0),
                            NomeLocalita = reader.GetString(1),
                            Indirizzo = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Comune = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            IdComune = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                            Latitudine = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5),
                            Longitudine = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6),
                            NomeResponsabile = reader.IsDBNull(7) ? "" : reader.GetString(7),
                            TelefonoResponsabile = reader.IsDBNull(8) ? "" : reader.GetString(8),
                            EmailResponsabile = reader.IsDBNull(9) ? "" : reader.GetString(9),
                            Note = reader.IsDBNull(10) ? "" : reader.GetString(10)
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log o gestione errore
            throw new Exception($"Errore durante il recupero delle località: {ex.Message}", ex);
        }

        return lista;
    }

    public async Task<bool> UpdateLocalitaAsync(Localita localita)
    {
        try
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(@"
            UPDATE Localita SET
                NomeLocalita = @NomeLocalita,
                Indirizzo = @Indirizzo,
                Comune = @Comune,
                IdComune = @IdComune,
                Latitudine = @Latitudine,
                Longitudine = @Longitudine,
                NomeResponsabile = @NomeResponsabile,
                TelefonoResponsabile = @TelefonoResponsabile,
                EmailResponsabile = @EmailResponsabile,
                Note = @Note
            WHERE IdLocalita = @IdLocalita", conn);

            // Aggiungi parametri
            cmd.Parameters.AddWithValue("@IdLocalita", localita.IdLocalita);
            cmd.Parameters.AddWithValue("@NomeLocalita", localita.NomeLocalita);
            cmd.Parameters.AddWithValue("@Indirizzo", (object)localita.Indirizzo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Comune", (object)localita.Comune ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IdComune", localita.IdComune > 0 ? (object)localita.IdComune : DBNull.Value);
            cmd.Parameters.AddWithValue("@Latitudine", localita.Latitudine != 0 ? (object)localita.Latitudine : DBNull.Value);
            cmd.Parameters.AddWithValue("@Longitudine", localita.Longitudine != 0 ? (object)localita.Longitudine : DBNull.Value);
            cmd.Parameters.AddWithValue("@NomeResponsabile", (object)localita.NomeResponsabile ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TelefonoResponsabile", (object)localita.TelefonoResponsabile ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EmailResponsabile", (object)localita.EmailResponsabile ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Note", (object)localita.Note ?? DBNull.Value);

            int affectedRows = await cmd.ExecuteNonQueryAsync();
            return affectedRows > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERRORE durante l'update di Localita ID {localita.IdLocalita}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteLocalitaAsync(int id)
    {
        try
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var cmd = new SqlCommand("DELETE FROM Localita WHERE IdLocalita = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);

                int affectedRows = await cmd.ExecuteNonQueryAsync();
                return affectedRows > 0; // true se ha eliminato almeno una riga
            }
        }
        catch (Exception ex)
        {
            // Log o gestione errore
            throw new Exception($"Errore durante l'eliminazione della località con ID {id}: {ex.Message}", ex);
        }
    }
}