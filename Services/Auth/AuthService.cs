using Microsoft.Data.SqlClient;
using VanGest.Server.Models.Login;

namespace VanGest.Server.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly string _connectionString;

        public AuthService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("StoricoDbConnection")
                ?? throw new ArgumentNullException("DefaultConnection non configurato");
        }

        public async Task<LoginResponse> Authenticate(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return LoginResponse.CreateFail("Username e password obbligatori");

            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                var operatorData = await GetOperatorData(conn, request.Username);
                if (operatorData == null || request.Password != operatorData.Password)
                    return LoginResponse.CreateFail("Credenziali non valide");

                return LoginResponse.CreateSuccess(
                    operatorData.IdOperatore,
                    operatorData.UserName,
                    "token_di_esempio"
                );
            }
            catch (Exception)
            {
                return LoginResponse.CreateFail("Errore interno");
            }
        }

        private async Task<OperatorData?> GetOperatorData(SqlConnection conn, string username)
        {
            const string query = "SELECT IdOperatore, UserGPS, PasswGPS FROM Operatori WHERE UserGPS = @Username";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Username", username);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            return new OperatorData
            {
                IdOperatore = Convert.ToInt32(reader["IdOperatore"]),
                UserName = reader["UserGPS"].ToString() ?? string.Empty,
                Password = reader["PasswGPS"].ToString() ?? string.Empty
            };
        }

        private class OperatorData
        {
            public int IdOperatore { get; set; }
            public string UserName { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
    }
}