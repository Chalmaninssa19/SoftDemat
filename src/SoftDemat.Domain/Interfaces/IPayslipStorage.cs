namespace SoftDemat.Domain.Interfaces;

public interface IPayslipDirectory
{
    IReadOnlyList<string> ListChildDirectories(string? relativeFolder);
    IReadOnlyList<string> ListPdfFileNames(string? relativeFolder);
    string ResolveFile(string? relativeFolder, string fileName);
}

public interface IPayslipArchiver
{
    void Archive(
        string sourceFile,
        string archiveRoot,
        DateTime payDate,
        string? establishmentName,
        string? mailCode,
        string matricule,
        string? firstName);
}
