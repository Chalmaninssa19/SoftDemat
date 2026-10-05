using Microsoft.EntityFrameworkCore;
using SoftDemat.Domain.Entities;
using SoftDemat.Domain.Exceptions;

namespace SoftDemat.Infrastructure.Context;

public sealed class SageDbContext : DbContext
{
    public SageDbContext(DbContextOptions<SageDbContext> options) : base(options)
    {
    }

    public DbSet<CurrentEmployee> Employees => Set<CurrentEmployee>();
    public DbSet<Establishment> Establishments => Set<Establishment>();

    public override int SaveChanges()
        => throw new DomainException("La base Sage est en lecture seule.");

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => throw new DomainException("La base Sage est en lecture seule.");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CurrentEmployee>(entity =>
        {
            entity.HasNoKey();
            entity.Ignore(employee => employee.FullName);
            entity.ToSqlQuery(
                """
                SELECT
                    LTRIM(RTRIM(s.MatriculeSalarie)) AS Matricule,
                    s.Nom AS LastName,
                    s.Prenom AS FirstName,
                    ISNULL(s.Email, '') AS Email,
                    LTRIM(RTRIM(ISNULL(e.CodeEtab, ''))) AS EstablishmentCode,
                    LTRIM(RTRIM(ISNULL(t.Intitule, ''))) AS EstablishmentName
                FROM T_SAL AS s
                LEFT JOIN T_HST_ETABLISSEMENT AS e
                    ON s.SA_CompteurNumero = e.NumSalarie AND e.DateHist IS NULL
                LEFT JOIN T_ETA AS t ON e.CodeEtab = t.CodeEtab
                """);
        });

        modelBuilder.Entity<Establishment>(entity =>
        {
            entity.HasNoKey();
            entity.ToSqlQuery(
                """
                SELECT LTRIM(RTRIM(CodeEtab)) AS Code, LTRIM(RTRIM(Intitule)) AS Name
                FROM T_ETA
                """);
        });
    }
}
