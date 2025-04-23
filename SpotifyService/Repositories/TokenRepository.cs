using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using SimplePresave.Libraries.Model;

namespace SimplePresave.Libraries.Repositories
{
    public class TokenRepository
    {
        private readonly string _connectionString;

        public TokenRepository(IOptions<AzureSettings> settings)
        {
            var connectionStrings = settings.Value;
            _connectionString = connectionStrings.AzureSqlConnectionString;
        }

        public async Task SaveTokens(string email, string accessToken, string refreshToken, DateTimeOffset expiresAt, decimal timeOffset)
        {
            const string insertQuery = @"
                INSERT INTO Tokens (Email, AccessToken, RefreshToken, ExpiresAt, TimeOffset, CreatedAt)
                VALUES (@Email, @AccessToken, @RefreshToken, @ExpiresAt, @TimeOffset, @CreatedAt)";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar) { Value = email });
                        command.Parameters.Add(new SqlParameter("@AccessToken", SqlDbType.NVarChar) { Value = accessToken });
                        command.Parameters.Add(new SqlParameter("@RefreshToken", SqlDbType.NVarChar) { Value = refreshToken });
                        command.Parameters.Add(new SqlParameter("@ExpiresAt", SqlDbType.DateTimeOffset) { Value = expiresAt });
                        command.Parameters.Add(new SqlParameter("@TimeOffset", SqlDbType.Decimal) { Value = timeOffset });
                        command.Parameters.Add(new SqlParameter("@CreatedAt", SqlDbType.DateTimeOffset) { Value = DateTimeOffset.UtcNow });

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                Console.WriteLine($"An error occurred while saving tokens: {ex.Message}");
                throw;
            }
        }
    }
}
