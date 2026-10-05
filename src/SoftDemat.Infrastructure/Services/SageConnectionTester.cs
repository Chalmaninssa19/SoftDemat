using Microsoft.Data.SqlClient;
using SoftDemat.Domain.Exceptions;
using SoftDemat.Domain.Interfaces;

namespace SoftDemat.Infrastructure.Services;

public sealed class SageConnectionTester : ISageConnectionTester
{
    public async Task TestAsync(
        string server,
        string databaseName,
        string? login,
        string? password,
        CancellationToken cancellationToken = default)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = server,
            InitialCatalog = databaseName,
            TrustServerCertificate = true,
            ConnectTimeout = 8
        };

        if (string.IsNullOrWhiteSpace(login))
            builder.IntegratedSecurity = true;
        else
        {
            builder.UserID = login;
            builder.Password = password ?? string.Empty;
        }

        try
        {
            await using var connection = new SqlConnection(builder.ConnectionString);
            await connection.OpenAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is SqlException or InvalidOperationException)
        {
            throw new DomainException("Connexion à la base Sage impossible.");
        }
    }
}
