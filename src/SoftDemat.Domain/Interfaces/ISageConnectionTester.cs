namespace SoftDemat.Domain.Interfaces;

public interface ISageConnectionTester
{
    Task TestAsync(string server, string databaseName, string? login, string? password, CancellationToken cancellationToken = default);
}
