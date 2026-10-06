namespace SoftDemat.Infrastructure.Options;

public sealed class PayslipStorageOptions
{
    public const string SectionName = "PayslipStorage";

    /// <summary>
    /// Racine de stockage. Vide : App_Data/payslip-uploads dans le répertoire de l'API.
    /// Chaque dépôt est un lot temporaire batches/{utilisateur}/{id}, conservé 24 heures.
    /// </summary>
    public string RootPath { get; set; } = string.Empty;
}
